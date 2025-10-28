#!/bin/bash

# SeatingChartApp Deployment Script for Digital Ocean
# Usage: ./deploy.sh [options]
# Options:
#   --backup    Create database backup before deploying
#   --fresh     Delete and recreate databases (CAUTION: deletes all data!)
#   --skip-services  Don't restart services (for testing)

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
CREATE_BACKUP=false
FRESH_START=false
SKIP_SERVICES=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --backup)
            CREATE_BACKUP=true
            shift
            ;;
        --fresh)
            FRESH_START=true
            shift
            ;;
        --skip-services)
            SKIP_SERVICES=true
            shift
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            echo "Usage: ./deploy.sh [--backup] [--fresh] [--skip-services]"
            exit 1
            ;;
    esac
done

# Print header
echo -e "${BLUE}================================================${NC}"
echo -e "${BLUE}  SeatingChartApp Deployment${NC}"
echo -e "${BLUE}================================================${NC}"
echo ""

# Step 1: Create backup if requested
if [ "$CREATE_BACKUP" = true ]; then
    echo -e "${YELLOW}📦 Creating database backups...${NC}"
    BACKUP_DATE=$(date +%Y%m%d_%H%M%S)
    
    if [ -f "lineup.db" ]; then
        cp lineup.db "lineup.db.backup.$BACKUP_DATE"
        echo -e "${GREEN}✓${NC} Backed up lineup.db"
    fi
    
    if [ -f "mealplanner.db" ]; then
        cp mealplanner.db "mealplanner.db.backup.$BACKUP_DATE"
        echo -e "${GREEN}✓${NC} Backed up mealplanner.db"
    fi
    
    # Cleanup old backups (keep last 5)
    ls -t lineup.db.backup.* 2>/dev/null | tail -n +6 | xargs -r rm
    ls -t mealplanner.db.backup.* 2>/dev/null | tail -n +6 | xargs -r rm
    echo ""
fi

# Step 2: Fresh start if requested (DANGER ZONE)
if [ "$FRESH_START" = true ]; then
    echo -e "${RED}⚠️  WARNING: Deleting all databases!${NC}"
    read -p "Are you sure? This will delete ALL data! (yes/no): " -r
    if [[ $REPLY == "yes" ]]; then
        rm -f lineup.db lineup.db-shm lineup.db-wal
        rm -f mealplanner.db mealplanner.db-shm mealplanner.db-wal
        echo -e "${GREEN}✓${NC} Databases deleted"
    else
        echo -e "${YELLOW}Cancelled fresh start${NC}"
    fi
    echo ""
fi

# Step 3: Pull latest code
echo "🔁 Pulling latest code from Git..."
git pull origin main || { echo -e "${RED}❌ Git pull failed${NC}"; exit 1; }
echo ""

# Step 4: Publish application
echo "🛠 Publishing app to /var/www/seatingchart/publish..."
dotnet publish -c Release -o /var/www/seatingchart/publish || { echo -e "${RED}❌ Publish failed${NC}"; exit 1; }
echo ""

# Step 5: Fix database permissions
if [ -f "lineup.db" ] || [ -f "mealplanner.db" ]; then
    echo -e "${YELLOW}🔐 Setting database permissions...${NC}"
    if [ -f "lineup.db" ]; then
        sudo chown www-data:www-data lineup.db
        sudo chmod 644 lineup.db
    fi
    if [ -f "mealplanner.db" ]; then
        sudo chown www-data:www-data mealplanner.db
        sudo chmod 644 mealplanner.db
    fi
    echo -e "${GREEN}✓${NC} Permissions set"
    echo ""
fi

# Step 6: Restart services
if [ "$SKIP_SERVICES" = false ]; then
    echo "🚀 Restarting services..."
    sudo systemctl restart seatingchart.service
    sudo systemctl restart voice_webhook.service
    sudo systemctl restart forward_sms.service
    echo ""

    echo "🔄 Reloading Nginx..."
    sudo systemctl reload nginx
    echo ""
    
    # Check if main service started successfully
    sleep 2
    if sudo systemctl is-active --quiet seatingchart.service; then
        echo -e "${GREEN}✓${NC} seatingchart.service is running"
    else
        echo -e "${RED}✗${NC} seatingchart.service failed to start"
        echo -e "${YELLOW}Checking logs:${NC}"
        sudo journalctl -u seatingchart.service -n 20 --no-pager
        exit 1
    fi
fi

# Success!
echo ""
echo -e "${GREEN}================================================${NC}"
echo -e "${GREEN}  ✅ Deployment complete!${NC}"
echo -e "${GREEN}================================================${NC}"
echo ""
echo -e "${BLUE}💡 Database migrations will run automatically on startup${NC}"
echo -e "${BLUE}💡 View logs: sudo journalctl -u seatingchart.service -f${NC}"

if [ "$CREATE_BACKUP" = true ]; then
    echo -e "${BLUE}💡 Backups saved with timestamp: $BACKUP_DATE${NC}"
fi
