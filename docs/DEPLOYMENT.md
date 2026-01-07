# LifeCraft Deployment Guide

## Overview

This guide covers deploying the LifeCraft backend API and Unity iOS client to production environments.

## Prerequisites

### Backend
- Node.js 18+
- PostgreSQL 15+
- Redis 7+
- PM2 (process manager) or Docker
- SSH access to production server
- Git access to repository

### iOS Client
- macOS with Xcode 15+
- Apple Developer account
- iOS device for testing
- TestFlight access for beta distribution

## Deployment Architecture

```
┌─────────────┐
│  GitHub     │
│  Repository │
└──────┬──────┘
       │ Push/PR
       │
┌──────▼──────────────┐
│  GitHub Actions      │
│  CI/CD Pipeline     │
└──────┬───────────────┘
       │ Build & Test
       │
┌──────▼────────────────┐
│  Staging Server       │
│  (Optional)          │
└───────────────────────┘
       │
       │ Manual Approve
       │
┌──────▼────────────────┐
│  Production Server   │
│  - PostgreSQL        │
│  - Redis            │
│  - Node.js API      │
└───────────────────────┘
```

## CI/CD Pipeline

### Workflow Triggers
- **Push to `main`**: Build, test, deploy to production
- **Push to `develop`**: Build, test, deploy to staging
- **Pull Request to `main`**: Build and test only

### Pipeline Stages

1. **Lint**: Code quality checks
   ```bash
   npm run lint
   ```

2. **Test**: Automated test suite
   ```bash
   docker-compose up -d
   npm test
   ```

3. **Build**: Compile TypeScript
   ```bash
   npm run build
   npx prisma generate
   ```

4. **Deploy**: SSH to server and deploy

### Required GitHub Secrets

Configure these in `Settings > Secrets and variables > Actions`:

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `STAGING_HOST` | Staging server hostname | `staging.lifecraft.com` |
| `STAGING_USER` | SSH username | `deploy` |
| `STAGING_SSH_KEY` | Private SSH key | `-----BEGIN RSA PRIVATE KEY-----` |
| `STAGING_PORT` | SSH port | `22` |
| `PRODUCTION_HOST` | Production server hostname | `api.lifecraft.com` |
| `PRODUCTION_USER` | SSH username | `deploy` |
| `PRODUCTION_SSH_KEY` | Private SSH key | `-----BEGIN RSA PRIVATE KEY-----` |
| `PRODUCTION_PORT` | SSH port | `22` |
| `SLACK_WEBHOOK` | Slack notification URL | `https://hooks.slack.com/...` |

## Manual Deployment

### Backend Deployment

#### 1. Prepare Server

SSH into your production server:

```bash
ssh user@your-server.com
```

#### 2. Install Dependencies

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Node.js 18
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# Install PM2
sudo npm install -g pm2

# Install PostgreSQL
sudo apt install -y postgresql postgresql-contrib

# Install Redis
sudo apt install -y redis-server
```

#### 3. Clone Repository

```bash
cd /var/www
sudo git clone https://github.com/srijanarya/Life-Sim.git lifecraft
cd lifecraft/server
```

#### 4. Configure Environment

```bash
cp .env.production.example .env
nano .env
```

Update with production values:
- DATABASE_URL
- REDIS_URL
- JWT_SECRET (generate secure random key)
- API_BASE_URL

Generate JWT secret:
```bash
node -e "console.log(require('crypto').randomBytes(32).toString('hex'))"
```

#### 5. Install Dependencies

```bash
npm ci --production
```

#### 6. Database Setup

```bash
# Generate Prisma client
npx prisma generate

# Run migrations
npx prisma migrate deploy

# Seed initial data
npm run db:seed
```

#### 7. Start Application with PM2

```bash
# Start application
pm2 start dist/index.js --name lifecraft-api

# Configure PM2 to start on boot
pm2 startup
pm2 save
```

PM2 ecosystem file:
```javascript
// ecosystem.config.js
module.exports = {
  apps: [{
    name: 'lifecraft-api',
    script: './dist/index.js',
    instances: 2,
    exec_mode: 'cluster',
    env: {
      NODE_ENV: 'production',
      PORT: 3000
    },
    error_file: './logs/err.log',
    out_file: './logs/out.log',
    log_date_format: 'YYYY-MM-DD HH:mm:ss Z',
    max_memory_restart: '1G'
  }]
};
```

Start with ecosystem:
```bash
pm2 start ecosystem.config.js
```

#### 8. Setup Nginx Reverse Proxy

```bash
sudo nano /etc/nginx/sites-available/lifecraft
```

Nginx configuration:
```nginx
server {
    listen 80;
    server_name api.lifecraft.com;

    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.lifecraft.com;

    ssl_certificate /etc/ssl/certs/lifecraft.crt;
    ssl_certificate_key /etc/ssl/certs/lifecraft.key;

    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
}
```

Enable site:
```bash
sudo ln -s /etc/nginx/sites-available/lifecraft /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

#### 9. Setup SSL Certificate

Using Let's Encrypt:
```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d api.lifecraft.com
```

Auto-renewal:
```bash
sudo certbot renew --dry-run
```

### Database Monitoring

#### PostgreSQL Backup

Create backup script `/var/scripts/backup-db.sh`:

```bash
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/var/backups/postgresql"
DB_NAME="lifecraft_prod"

mkdir -p $BACKUP_DIR

pg_dump -Fc $DB_NAME > $BACKUP_DIR/lifecraft_$DATE.dump

# Keep last 7 days
find $BACKUP_DIR -name "lifecraft_*.dump" -mtime +7 -delete
```

Cron job for daily backups:
```bash
sudo crontab -e
# Add line:
0 2 * * * /var/scripts/backup-db.sh
```

#### Redis Configuration

Edit `/etc/redis/redis.conf`:
```
maxmemory 256mb
maxmemory-policy allkeys-lru
save 900 1
save 300 10
save 60 10000
```

Restart Redis:
```bash
sudo systemctl restart redis
```

## Monitoring and Logging

### Application Monitoring

#### PM2 Monitoring

Monitor application logs:
```bash
pm2 logs lifecraft-api
pm2 monit
```

Web-based monitoring:
```bash
pm2 install @pm2/io-plugin
pm2 install @pm2/pm2-logrotate
```

#### Health Check Endpoint

Health endpoint: `GET /health`

Response:
```json
{
  "status": "healthy",
  "timestamp": "2026-01-07T12:00:00Z",
  "uptime": 3600,
  "database": "connected",
  "redis": "connected"
}
```

Nginx health check:
```bash
curl http://localhost:3000/health
```

### Log Rotation

PM2 log rotation:
```javascript
// ecosystem.config.js
module.exports = {
  apps: [{
    name: 'lifecraft-api',
    script: './dist/index.js',
    error_file: './logs/err.log',
    out_file: './logs/out.log',
    log_date_format: 'YYYY-MM-DD HH:mm:ss Z',
    merge_logs: true,
    max_memory_restart: '1G',
    env: {
      NODE_ENV: 'production'
    }
  }, {
    pm2: {
      name: 'lifecraft-api',
      cron: '0 0 * * *',
      script: 'pm2 flush',
      log_date_format: 'YYYY-MM-DD HH:mm:ss Z'
    }
  }]
};
```

Nginx log rotation:
```bash
sudo nano /etc/logrotate.d/nginx
```

```
/var/log/nginx/*.log {
    daily
    missingok
    rotate 14
    compress
    delaycompress
    notifempty
    create 0640 www-data adm
    sharedscripts
        postrotate
            [ -f /var/run/nginx.pid ] && kill -USR1 `cat /var/run/nginx.pid`
    endscript
}
```

## Security Hardening

### Firewall Configuration

```bash
# Configure UFW
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh
sudo ufw allow http
sudo ufw allow https
sudo ufw enable
```

### Rate Limiting

Already configured in `index.ts`. Verify:

```javascript
rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100,
  message: 'Too many requests from this IP'
});
```

### Environment Security

1. **Never commit `.env` files**
2. **Use strong JWT secrets**: 32+ random characters
3. **Enable database SSL**: `DATABASE_URL` must use `postgresql://` with SSL
4. **Rotate secrets regularly**: Every 90 days
5. **Monitor for vulnerabilities**: Regular security audits

## iOS Client Deployment

### Building for Production

1. Open Unity project
2. Go to `File > Build Settings`
3. Configure:
   - Platform: iOS
   - Team: Your Apple Developer Team
   - Bundle Identifier: `com.lifecraft.game`
   - Version: Sync with backend
   - Build Number: Increment for each release

4. Build:
   - Development: Run in Editor
   - Production: Build for App Store distribution

### App Store Submission

1. **Prepare Metadata**:
   - App name: LifeCraft
   - Description (28+ languages)
   - Keywords
   - Screenshots (multiple devices)
   - Icon (1024x1024)

2. **Configure In-App Purchases**:
   - Products: Currency packs, VIP subscriptions
   - Prices: Set per region
   - Promotions: Discount codes

3. **Privacy Policy**:
   - URL required in App Store Connect
   - Include data collection and usage

4. **Submit for Review**:
   - Upload build (.ipa)
   - Wait for Apple review (1-3 days)
   - Respond to rejection feedback promptly

### TestFlight Beta

1. **Create Beta Group**: Internal or external testers
2. **Upload to TestFlight**: Via App Store Connect
3. **Invite Testers**: Share TestFlight link
4. **Monitor Crashes**: Use Xcode Organizer

## Rollback Procedures

### Backend Rollback

#### Quick Rollback (PM2)

```bash
# Check previous version
pm2 list

# Restart previous version
pm2 restart lifecraft-api

# Or reload from Git
cd /var/www/lifecraft
git log --oneline -5
git revert HEAD
pm2 reload lifecraft-api
```

#### Database Rollback

```bash
# Restore from backup
pg_restore -d lifecraft_prod /var/backups/postgresql/lifecraft_YYYYMMDD_HHMMSS.dump
```

### iOS Rollback

1. **App Store**: Cannot roll back, must submit new version
2. **TestFlight**: Upload previous build
3. **Emergency**: Disable problematic features via backend feature flags

## Troubleshooting

### Common Issues

#### Port Already in Use

```bash
# Find process using port 3000
sudo lsof -i :3000

# Kill process
sudo kill -9 <PID>

# Or change PORT in .env
PORT=3001
```

#### Database Connection Failed

```bash
# Check PostgreSQL status
sudo systemctl status postgresql

# Check connection string
psql $DATABASE_URL

# Restart if needed
sudo systemctl restart postgresql
```

#### Out of Memory

```bash
# Check Node.js memory usage
pm2 monit

# Increase PM2 memory limit
pm2 restart lifecraft-api --max-memory-restart 1G

# Or use cluster mode in ecosystem.config.js
instances: 4  # Use 4 instances
```

### Performance Optimization

#### Enable Gzip Compression

```nginx
gzip on;
gzip_vary on;
gzip_min_length 1024;
gzip_types text/plain text/css text/xml application/json application/javascript;
```

#### Enable CDN (Optional)

For static assets:
```
Nginx -> CDN -> User
```

Configure CDN in `.env`:
```bash
CDN_URL=https://cdn.lifecraft.com
```

## Scaling

### Vertical Scaling

Increase server resources:
- CPU: 2 → 4 cores
- RAM: 4GB → 8GB
- SSD: 50GB → 100GB

### Horizontal Scaling

Use load balancer:

```
Users → Load Balancer → [Server 1, Server 2, Server 3]
```

PM2 cluster mode:
```javascript
instances: 'max',  // Use all CPU cores
exec_mode: 'cluster'
```

## Support and Maintenance

### Regular Tasks

**Daily**:
- Check PM2 logs
- Monitor error rates
- Verify database backups

**Weekly**:
- Review performance metrics
- Check for security updates
- Analyze slow queries

**Monthly**:
- Rotate secrets (JWT, API keys)
- Review and update dependencies
- Audit access logs
- Storage cleanup

### Contact Information

- **Deployment Issues**: devops@lifecraft.com
- **Backend Bugs**: backend-team@lifecraft.com
- **iOS Issues**: ios-team@lifecraft.com
- **Emergency Pager**: +1-555-LIFECRAFT

---

**Last Updated**: January 7, 2026
**Version**: 1.0.0
