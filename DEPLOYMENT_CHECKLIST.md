# IntelliPM Deployment Checklist

This checklist ensures a smooth deployment of IntelliPM to production environments.

## Pre-Deployment

### Backend Configuration

- [ ] **JWT Secret Key**
  - [ ] Generate a secure random key (at least 32 characters)
  - [ ] Set `Jwt:SecretKey` in `appsettings.json` or via `Jwt__SecretKey` environment variable
  - [ ] Verify key is not committed to version control

- [ ] **Database Connection Strings**
  - [ ] Update `ConnectionStrings:SqlServer` with production database
  - [ ] Update `ConnectionStrings:VectorDb` with production PostgreSQL instance
  - [ ] Verify database credentials are correct
  - [ ] Test database connectivity

- [ ] **SMTP Email Configuration**
  - [ ] Set `Email:SmtpHost` to production SMTP server
  - [ ] Set `Email:SmtpPort` (typically 587 for TLS, 465 for SSL)
  - [ ] Set `Email:SmtpUsername` and `Email:SmtpPassword`
  - [ ] Set `Email:FromEmail` to verified sender address
  - [ ] Set `Email:FromName` to desired sender name
  - [ ] Verify SMTP health check at `/api/v1/health`

- [ ] **Security Settings**
  - [ ] Set `SecurityHeaders:EnableHSTS` to `true` if using HTTPS
  - [ ] Configure `AllowedOrigins` with production frontend URL(s)
  - [ ] Review and adjust `RateLimiting` settings if needed

- [ ] **Super Admin Account**
  - [ ] Change `SuperAdmin:Email` from default
  - [ ] Change `SuperAdmin:Password` to a strong password
  - [ ] Verify `SuperAdmin:Username` is appropriate

- [ ] **Error Tracking**
  - [ ] Set `Sentry:Dsn` if using Sentry
  - [ ] Set `Sentry:Environment` to "production"
  - [ ] Adjust `Sentry:TracesSampleRate` to 0.1 (10%) for production

- [ ] **Frontend URL**
  - [ ] Set `Frontend:BaseUrl` to production frontend URL

- [ ] **Build Configuration**
  - [ ] Verify `TestController` is excluded in Release builds (uses `#if DEBUG`)
  - [ ] Build in Release mode: `dotnet build -c Release`
  - [ ] Verify no DEBUG-only code is included

### Frontend Configuration

- [ ] **Environment Variables**
  - [ ] Create `.env` file from `.env.example`
  - [ ] Set `VITE_API_BASE_URL` to production backend URL
  - [ ] Set `VITE_SENTRY_DSN` if using Sentry
  - [ ] Set `VITE_SENTRY_ENVIRONMENT` to "production"

- [ ] **Build**
  - [ ] Run `npm run build` or `npm run build:dev` for production build
  - [ ] Verify build output in `dist/` directory
  - [ ] Test production build locally with `npm run preview`

## Deployment

### Backend Deployment

- [ ] **Database Migrations**
  - [ ] Run database migrations: `dotnet ef database update`
  - [ ] Verify migrations applied successfully
  - [ ] Check for pending migrations

- [ ] **Application Startup**
  - [ ] Verify application starts without errors
  - [ ] Check startup logs for SMTP configuration warnings
  - [ ] Verify health checks respond correctly

- [ ] **Health Checks**
  - [ ] Test `/api/v1/health` endpoint
  - [ ] Verify SMTP health check passes
  - [ ] Verify database health check passes
  - [ ] Check `/api/health/ready` for readiness probe

### Frontend Deployment

- [ ] **Static Files**
  - [ ] Deploy `dist/` directory to web server
  - [ ] Configure web server for SPA routing (all routes → `index.html`)
  - [ ] Verify static assets are served correctly

- [ ] **CORS Configuration**
  - [ ] Verify backend `AllowedOrigins` includes frontend URL
  - [ ] Test CORS with actual frontend domain

## Post-Deployment Verification

### Functional Testing

- [ ] **Authentication**
  - [ ] Test user login
  - [ ] Test user registration (if enabled)
  - [ ] Test password reset flow
  - [ ] Verify JWT tokens are issued correctly

- [ ] **Email Functionality**
  - [ ] Test password reset email
  - [ ] Test user invitation email
  - [ ] Verify emails are delivered
  - [ ] Check email formatting and links

- [ ] **API Endpoints**
  - [ ] Test critical API endpoints
  - [ ] Verify error handling works correctly
  - [ ] Check rate limiting is enforced

- [ ] **Health Monitoring**
  - [ ] Verify health check endpoints are accessible
  - [ ] Set up monitoring for health check endpoints
  - [ ] Configure alerts for unhealthy status

### Security Verification

- [ ] **HTTPS**
  - [ ] Verify HTTPS is enabled
  - [ ] Check SSL certificate is valid
  - [ ] Verify HSTS header is present (if enabled)

- [ ] **Security Headers**
  - [ ] Verify security headers are present
  - [ ] Check CSP headers
  - [ ] Verify X-Frame-Options header

- [ ] **Authentication**
  - [ ] Verify JWT tokens expire correctly
  - [ ] Test token refresh flow
  - [ ] Verify unauthorized access is blocked

### Performance Testing

- [ ] **Load Testing**
  - [ ] Test API under expected load
  - [ ] Verify rate limiting works correctly
  - [ ] Check response times are acceptable

- [ ] **Database Performance**
  - [ ] Monitor database query performance
  - [ ] Check for slow queries
  - [ ] Verify connection pooling is working

## Monitoring Setup

- [ ] **Error Tracking**
  - [ ] Verify Sentry is capturing errors (if configured)
  - [ ] Set up error alerting
  - [ ] Configure error notification rules

- [ ] **Logging**
  - [ ] Verify logs are being written
  - [ ] Set up log aggregation (if using Seq or similar)
  - [ ] Configure log retention policies

- [ ] **Health Monitoring**
  - [ ] Set up health check monitoring
  - [ ] Configure alerts for health check failures
  - [ ] Set up uptime monitoring

## Rollback Plan

- [ ] **Backup Strategy**
  - [ ] Database backups are configured
  - [ ] Backup retention policy is set
  - [ ] Test restore procedure

- [ ] **Rollback Procedure**
  - [ ] Document rollback steps
  - [ ] Test rollback procedure
  - [ ] Ensure previous version artifacts are available

## Documentation

- [ ] **Configuration Documentation**
  - [ ] Document all environment-specific settings
  - [ ] Update `CONFIGURATION.md` with production values (without secrets)
  - [ ] Document any custom configurations

- [ ] **Runbook**
  - [ ] Create runbook for common issues
  - [ ] Document troubleshooting steps
  - [ ] Include contact information for support

## Notes

- Keep this checklist updated as deployment process evolves
- Review and update before each major deployment
- Document any environment-specific steps not covered here

