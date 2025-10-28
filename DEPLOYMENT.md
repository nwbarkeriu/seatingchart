# Digital Ocean Deployment Guide

## Quick Deploy Steps

### 1. Pull Latest Code on Digital Ocean
```bash
cd /path/to/your/app
git pull origin main
```

### 2. Build the Application
```bash
dotnet build --configuration Release
```

### 3. Run the Application
```bash
dotnet run --configuration Release
# OR if using systemd service
sudo systemctl restart seatingchartapp
```

## What Happens Automatically

✅ **Database migrations run automatically** on startup via `Program.cs`
- `lineup.db` will be created/updated with latest schema
- `mealplanner.db` will be created/updated with latest schema

✅ **No manual migration commands needed** - just pull and restart!

## Deployment Workflow

1. **Local Development:**
   - Make changes
   - Test locally
   - Commit to Git: `git add . && git commit -m "your message"`
   - Push to GitHub: `git push origin main`

2. **Digital Ocean Deployment:**
   ```bash
   ssh your-droplet
   cd /path/to/app
   git pull origin main
   dotnet build -c Release
   sudo systemctl restart seatingchartapp  # or however you run it
   ```

3. **Verify:**
   - Migrations apply automatically on startup
   - Check logs: `sudo journalctl -u seatingchartapp -f`

## Database Files

Your SQLite databases will be created at:
- `/path/to/app/lineup.db`
- `/path/to/app/mealplanner.db`

**Important:** Make sure these files have write permissions for the application user!

```bash
# If needed, fix permissions:
sudo chown www-data:www-data *.db
sudo chmod 644 *.db
```

## Troubleshooting

### Migration Errors
If you see migration errors, you can manually apply them:
```bash
dotnet ef database update --context LineupDbContext
dotnet ef database update --context MealPlannerDbContext
```

### Database Locked
If SQLite shows "database is locked":
```bash
# Stop the app
sudo systemctl stop seatingchartapp

# Check for stale lock files
rm -f *.db-shm *.db-wal

# Restart
sudo systemctl start seatingchartapp
```

### Fresh Start (Nuclear Option)
If you need to completely recreate databases:
```bash
# CAUTION: This deletes all data!
sudo systemctl stop seatingchartapp
rm -f lineup.db lineup.db-shm lineup.db-wal
rm -f mealplanner.db mealplanner.db-shm mealplanner.db-wal
sudo systemctl start seatingchartapp
```

## Backup Strategy

**Before deploying major changes:**
```bash
# Backup databases
cp lineup.db lineup.db.backup.$(date +%Y%m%d)
cp mealplanner.db mealplanner.db.backup.$(date +%Y%m%d)
```

## Environment Variables (Optional)

If you want different database paths in production, create `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "LineupConnection": "Data Source=/var/lib/seatingchart/lineup.db",
    "MealPlannerConnection": "Data Source=/var/lib/seatingchart/mealplanner.db"
  }
}
```

## First-Time Setup on Digital Ocean

If this is your first deployment:

1. **Install .NET Runtime:**
   ```bash
   wget https://dot.net/v1/dotnet-install.sh
   chmod +x dotnet-install.sh
   ./dotnet-install.sh --channel 9.0 --runtime aspnetcore
   ```

2. **Clone Repository:**
   ```bash
   git clone https://github.com/nwbarkeriu/seatingchart.git
   cd seatingchart
   ```

3. **Build and Run:**
   ```bash
   dotnet build -c Release
   dotnet run -c Release
   ```

4. **Set up as a Service (Optional):**
   Create `/etc/systemd/system/seatingchartapp.service`:
   ```ini
   [Unit]
   Description=Seating Chart App
   After=network.target

   [Service]
   Type=notify
   WorkingDirectory=/path/to/seatingchart
   ExecStart=/usr/bin/dotnet /path/to/seatingchart/bin/Release/net9.0/SeatingChartApp.dll
   Restart=always
   RestartSec=10
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

   [Install]
   WantedBy=multi-user.target
   ```

   Then:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable seatingchartapp
   sudo systemctl start seatingchartapp
   ```

## Summary

**No special migration scripts needed!** Just:
1. Push to GitHub from your local machine
2. Pull on Digital Ocean
3. Restart the app
4. Migrations run automatically ✨
