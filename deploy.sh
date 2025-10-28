#!/bin/bash

# SeatingChartApp Deployment Script for Digital Ocean
# Usage: ./deploy.sh [options]
# Options:
#   --backup    Create database backup before deploying
#   --fresh     Delete and recreate databases (CAUTION: deletes all data!)
#   --publish   Use dotnet publish instead of dotnet build

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration - UPDATE THESE FOR YOUR DROPLET
APP_DIR="/var/www/seatingchartapp"
SERVICE_NAME="seatingchartapp"
APP_USER="www-data"
USE_PUBLISH=false
CREATE_BACKUP=false
FRESH_START=false

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
        --publish)
            USE_PUBLISH=true
            shift
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            echo "Usage: ./deploy.sh [--backup] [--fresh] [--publish]"
            exit 1
            ;;
    esac
done

# Print header
echo -e "${BLUE}================================================${NC}"
echo -e "${BLUE}  SeatingChartApp Deployment Script${NC}"
echo -e "${BLUE}================================================${NC}"
echo ""

# Step 1: Check if we're in the right directory
if [ ! -f "SeatingChartApp.csproj" ]; then
    echo -e "${RED}Error: SeatingChartApp.csproj not found!${NC}"
    echo -e "${YELLOW}Please run this script from the app directory${NC}"
    exit 1
fi

echo -e "${GREEN}✓${NC} Found SeatingChartApp.csproj"

# Step 2: Create backup if requested
if [ "$CREATE_BACKUP" = true ]; then
    echo ""
    echo -e "${YELLOW}Creating database backups...${NC}"
    BACKUP_DATE=$(date +%Y%m%d_%H%M%S)
    
    if [ -f "lineup.db" ]; then
        cp lineup.db "lineup.db.backup.$BACKUP_DATE"
        echo -e "${GREEN}✓${NC} Backed up lineup.db"
    fi
    
    if [ -f "mealplanner.db" ]; then
        cp mealplanner.db "mealplanner.db.backup.$BACKUP_DATE"
        echo -e "${GREEN}✓${NC} Backed up mealplanner.db"
    fi
fi

# Step 3: Stop the service
echo ""
echo -e "${YELLOW}Stopping $SERVICE_NAME service...${NC}"
if sudo systemctl is-active --quiet $SERVICE_NAME; then
    sudo systemctl stop $SERVICE_NAME
    echo -e "${GREEN}✓${NC} Service stopped"
else
    echo -e "${YELLOW}⚠${NC} Service was not running"
fi

# Step 4: Fresh start if requested (DANGER ZONE)
if [ "$FRESH_START" = true ]; then
    echo ""
    echo -e "${RED}⚠️  WARNING: Deleting all databases!${NC}"
    read -p "Are you sure? This will delete ALL data! (yes/no): " -r
    if [[ $REPLY == "yes" ]]; then
        rm -f lineup.db lineup.db-shm lineup.db-wal
        rm -f mealplanner.db mealplanner.db-shm mealplanner.db-wal
        echo -e "${GREEN}✓${NC} Databases deleted"
    else
        echo -e "${YELLOW}Cancelled fresh start${NC}"
    fi
fi

# Step 5: Pull latest code
echo ""
echo -e "${YELLOW}Pulling latest code from GitHub...${NC}"
git fetch origin
LOCAL=$(git rev-parse @)
REMOTE=$(git rev-parse @{u})

if [ $LOCAL = $REMOTE ]; then
    echo -e "${YELLOW}⚠${NC} Already up to date"
else
    git pull origin main
    echo -e "${GREEN}✓${NC} Code updated"
fi

# Step 6: Build or Publish
echo ""
if [ "$USE_PUBLISH" = true ]; then
    echo -e "${YELLOW}Publishing application...${NC}"
    dotnet publish --configuration Release --output ./publish
    echo -e "${GREEN}✓${NC} Application published to ./publish"
else
    echo -e "${YELLOW}Building application...${NC}"
    dotnet build --configuration Release
    echo -e "${GREEN}✓${NC} Application built"
fi

# Step 7: Fix database permissions
echo ""
echo -e "${YELLOW}Setting database permissions...${NC}"
if [ -f "lineup.db" ]; then
    sudo chown $APP_USER:$APP_USER lineup.db
    sudo chmod 644 lineup.db
fi
if [ -f "mealplanner.db" ]; then
    sudo chown $APP_USER:$APP_USER mealplanner.db
    sudo chmod 644 mealplanner.db
fi
echo -e "${GREEN}✓${NC} Permissions set"

# Step 8: Start the service
echo ""
echo -e "${YELLOW}Starting $SERVICE_NAME service...${NC}"
sudo systemctl start $SERVICE_NAME

# Wait a moment for startup
sleep 2

# Step 9: Check service status
if sudo systemctl is-active --quiet $SERVICE_NAME; then
    echo -e "${GREEN}✓${NC} Service started successfully"
    echo ""
    echo -e "${GREEN}================================================${NC}"
    echo -e "${GREEN}  Deployment Complete!${NC}"
    echo -e "${GREEN}================================================${NC}"
    echo ""
    echo -e "Service status:"
    sudo systemctl status $SERVICE_NAME --no-pager | head -n 10
    echo ""
    echo -e "${BLUE}Tip: View logs with:${NC}"
    echo -e "  sudo journalctl -u $SERVICE_NAME -f"
else
    echo -e "${RED}✗${NC} Service failed to start"
    echo ""
    echo -e "${RED}Error details:${NC}"
    sudo journalctl -u $SERVICE_NAME -n 50 --no-pager
    exit 1
fi

# Step 10: Show database migration info
echo ""
echo -e "${BLUE}Database migrations will run automatically on startup${NC}"
echo -e "${BLUE}Check the logs above for any migration messages${NC}"

# Step 11: Cleanup old backups (keep last 5)
if [ "$CREATE_BACKUP" = true ]; then
    echo ""
    echo -e "${YELLOW}Cleaning up old backups (keeping last 5)...${NC}"
    ls -t lineup.db.backup.* 2>/dev/null | tail -n +6 | xargs -r rm
    ls -t mealplanner.db.backup.* 2>/dev/null | tail -n +6 | xargs -r rm
    echo -e "${GREEN}✓${NC} Old backups cleaned"
fi

echo ""
echo -e "${GREEN}🚀 Deployment finished successfully!${NC}"
