# Email Configuration Status Report
**Generated:** 2025-01-02  
**Task ID:** INTELLIPM-T003  
**Scope:** Backend - Verify SMTP configuration placeholders and email service DI registration

---

## Executive Summary

✅ **Email configuration structure verified**  
✅ **DI registration logic documented**  
✅ **All required configuration keys identified**  
⚠️ **SMTP credentials are placeholders** (expected - must be configured for production)

---

## Current Email Configuration Structure

### Configuration File: `appsettings.json`

**Location:** `backend/IntelliPM.API/appsettings.json` (lines 87-96)

```json
{
  "Email": {
    "Provider": "SMTP",
    "SmtpHost": "smtp-relay.brevo.com",
    "SmtpPort": 587,
    "SmtpUsername": "PLACEHOLDER",
    "SmtpPassword": "PLACEHOLDER",
    "FromEmail": "mohamedelmahdi.touimy@gmail.com",
    "FromName": "IntelliPM",
    "EnableSsl": true
  }
}
```

### Configuration Keys Breakdown

| Key | Type | Required | Default | Current Value | Status |
|-----|------|----------|---------|---------------|--------|
| `Email:Provider` | string | Yes | N/A | `"SMTP"` | ✅ Set |
| `Email:SmtpHost` | string | Yes | `"smtp.gmail.com"` | `"smtp-relay.brevo.com"` | ✅ Set |
| `Email:SmtpPort` | int | Yes | `587` | `587` | ✅ Set |
| `Email:SmtpUsername` | string | Yes* | `""` | `"PLACEHOLDER"` | ⚠️ Placeholder |
| `Email:SmtpPassword` | string | Yes* | `""` | `"PLACEHOLDER"` | ⚠️ Placeholder |
| `Email:FromEmail` | string | Yes | `"noreply@intellipm.com"` | `"mohamedelmahdi.touimy@gmail.com"` | ✅ Set |
| `Email:FromName` | string | Yes | `"IntelliPM"` | `"IntelliPM"` | ✅ Set |
| `Email:EnableSsl` | bool | No | `true` | `true` | ✅ Set |
| `Email:SecureSocketOptions` | string | No | N/A | Not set | ⚠️ Optional |

**Note:** `SmtpUsername` and `SmtpPassword` are marked as "Yes*" because they are required for SMTP authentication, but the service will fall back to stub mode if not provided.

### Additional Configuration Keys (Optional)

- `Email:SecureSocketOptions` - Optional override for MailKit SecureSocketOptions (e.g., "Auto", "SslOnConnect", "StartTls", "StartTlsWhenAvailable")
  - **Location:** Read in `SmtpEmailService.cs` line 45
  - **Usage:** Overrides automatic socket options detection based on port
  - **Default:** Auto-detected from port (587 = StartTls, 465 = SslOnConnect, 25 = None)

---

## Dependency Injection Registration Logic

### File: `backend/IntelliPM.Infrastructure/DependencyInjection.cs`

**Lines:** 97-106

```csharp
// Email Service - Use SMTP if configured, otherwise use stub EmailService
var emailProvider = config["Email:Provider"];
if (emailProvider == "SMTP" && !string.IsNullOrEmpty(config["Email:SmtpUsername"]))
{
    services.AddScoped<IEmailService, SmtpEmailService>();
}
else
{
    services.AddScoped<IEmailService, EmailService>();
}
```

### DI Logic Flow

1. **Read `Email:Provider`** from configuration
2. **Check if Provider == "SMTP"** AND `Email:SmtpUsername` is not null/empty
3. **If both conditions true:**
   - Register `SmtpEmailService` as `IEmailService`
   - Emails will be sent via SMTP
4. **If either condition false:**
   - Register `EmailService` (stub) as `IEmailService`
   - Emails will be logged but not sent

### Current Status

- ✅ `Email:Provider` = `"SMTP"` (condition 1 met)
- ⚠️ `Email:SmtpUsername` = `"PLACEHOLDER"` (condition 2 NOT met - empty string check fails)
- **Result:** Currently using **stub `EmailService`** (emails logged, not sent)

### To Enable SMTP

Set `Email:SmtpUsername` to a non-empty value (not "PLACEHOLDER"):

```json
{
  "Email": {
    "SmtpUsername": "your-actual-username@example.com"
  }
}
```

---

## Required Environment Variables for Production SMTP Setup

### Option 1: Environment Variables (Recommended for Production)

Use environment variables with double underscore (`__`) or colon (`:`) notation:

```bash
# Windows PowerShell
$env:Email__Provider="SMTP"
$env:Email__SmtpHost="smtp.gmail.com"
$env:Email__SmtpPort="587"
$env:Email__SmtpUsername="your-email@gmail.com"
$env:Email__SmtpPassword="your-app-password"
$env:Email__FromEmail="noreply@intellipm.com"
$env:Email__FromName="IntelliPM"
$env:Email__EnableSsl="true"

# Linux/Mac Bash
export Email__Provider="SMTP"
export Email__SmtpHost="smtp.gmail.com"
export Email__SmtpPort="587"
export Email__SmtpUsername="your-email@gmail.com"
export Email__SmtpPassword="your-app-password"
export Email__FromEmail="noreply@intellipm.com"
export Email__FromName="IntelliPM"
export Email__EnableSsl="true"
```

### Option 2: Docker Compose Environment Variables

Add to `docker-compose.yml` backend service:

```yaml
backend:
  environment:
    - Email__Provider=SMTP
    - Email__SmtpHost=smtp.gmail.com
    - Email__SmtpPort=587
    - Email__SmtpUsername=${SMTP_USERNAME}
    - Email__SmtpPassword=${SMTP_PASSWORD}
    - Email__FromEmail=noreply@intellipm.com
    - Email__FromName=IntelliPM
    - Email__EnableSsl=true
```

Then set in `.env` file:
```env
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
```

### Option 3: User Secrets (Development Only)

```bash
dotnet user-secrets set "Email:SmtpUsername" "your-email@gmail.com" --project IntelliPM.API
dotnet user-secrets set "Email:SmtpPassword" "your-app-password" --project IntelliPM.API
```

---

## SMTP Provider Examples

### Gmail SMTP

```json
{
  "Email": {
    "Provider": "SMTP",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "IntelliPM",
    "EnableSsl": true
  }
}
```

**Important Notes:**
- Use **App Password** (not regular password) for Gmail
- `FromEmail` should match `SmtpUsername` to avoid spam filtering
- Enable "Less secure app access" or use App Passwords

### Brevo (Sendinblue) SMTP

```json
{
  "Email": {
    "Provider": "SMTP",
    "SmtpHost": "smtp-relay.brevo.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-brevo-username",
    "SmtpPassword": "your-brevo-smtp-key",
    "FromEmail": "verified-email@yourdomain.com",
    "FromName": "IntelliPM",
    "EnableSsl": true
  }
}
```

**Important Notes:**
- `FromEmail` must be verified in Brevo dashboard
- Use SMTP key from Brevo account settings
- Port 587 uses StartTLS automatically

### Microsoft 365 / Outlook SMTP

```json
{
  "Email": {
    "Provider": "SMTP",
    "SmtpHost": "smtp.office365.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@outlook.com",
    "SmtpPassword": "your-password",
    "FromEmail": "your-email@outlook.com",
    "FromName": "IntelliPM",
    "EnableSsl": true
  }
}
```

### Custom SMTP Server

```json
{
  "Email": {
    "Provider": "SMTP",
    "SmtpHost": "mail.yourdomain.com",
    "SmtpPort": 587,
    "SmtpUsername": "noreply@yourdomain.com",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "IntelliPM",
    "EnableSsl": true,
    "SecureSocketOptions": "StartTls"
  }
}
```

---

## SmtpEmailService Implementation Details

### Configuration Reading

**File:** `backend/IntelliPM.Infrastructure/Services/SmtpEmailService.cs`

**Constructor (lines 29-77):**
- Reads configuration from `IConfiguration` via `GetSection("Email")`
- Defaults:
  - `SmtpHost`: `"smtp.gmail.com"` (if not set)
  - `SmtpPort`: `587` (if not set)
  - `FromEmail`: `"noreply@intellipm.com"` (if not set)
  - `FromName`: `"IntelliPM"` (if not set)
  - `EnableSsl`: `true` (if not set)

### Socket Options Logic

**File:** `backend/IntelliPM.Infrastructure/Services/SmtpSocketOptionsHelper.cs` (referenced)

Socket options are determined by:
1. **Port-based defaults:**
   - Port `587` → `StartTls`
   - Port `465` → `SslOnConnect`
   - Port `25` → `None`
2. **Optional override:** `Email:SecureSocketOptions` config key
3. **Certificate revocation:** Disabled for compatibility (`CheckCertificateRevocation = false`)

### Email Sending Logic

**Method:** `SendEmailAsync` (lines 304-367)

1. **Checks credentials:**
   - If `SmtpUsername` or `SmtpPassword` is empty → logs warning and returns (no email sent)
2. **Creates MimeMessage** with HTML body
3. **Connects to SMTP server** with determined socket options
4. **Authenticates** if credentials provided
5. **Sends email** and disconnects

---

## Email Service Methods

### IEmailService Interface Methods

All methods are implemented in both `EmailService` (stub) and `SmtpEmailService`:

1. ✅ `SendInvitationEmailAsync` - Project member invitations
2. ✅ `SendPasswordResetEmailAsync` - Password reset links
3. ✅ `SendWelcomeEmailAsync` - New user welcome emails
4. ✅ `SendOrganizationInvitationEmailAsync` - Organization invitations
5. ✅ `SendTestEmailAsync` - Test email functionality
6. ✅ `SendAIQuotaUpdatedEmailAsync` - AI quota tier change notifications
7. ✅ `SendAIDisabledNotificationAsync` - AI disabled notifications
8. ⚠️ `SendMentionNotificationEmailAsync` - **NOT IMPLEMENTED** (see audit report)

---

## Verification Checklist

### Configuration Keys Present

- [x] `Email:Provider` exists in `appsettings.json`
- [x] `Email:SmtpHost` exists in `appsettings.json`
- [x] `Email:SmtpPort` exists in `appsettings.json`
- [x] `Email:SmtpUsername` exists in `appsettings.json` (placeholder)
- [x] `Email:SmtpPassword` exists in `appsettings.json` (placeholder)
- [x] `Email:FromEmail` exists in `appsettings.json`
- [x] `Email:FromName` exists in `appsettings.json`
- [x] `Email:EnableSsl` exists in `appsettings.json`
- [x] `Email:SecureSocketOptions` is optional (not in appsettings.json, but supported)

### DI Registration Logic

- [x] DI registration checks `Email:Provider == "SMTP"`
- [x] DI registration checks `Email:SmtpUsername` is not empty
- [x] `SmtpEmailService` registered when both conditions met
- [x] `EmailService` (stub) registered as fallback
- [x] Logic is in `DependencyInjection.cs` lines 98-106

### Current Status

- ✅ Configuration structure is correct
- ✅ DI logic is correct
- ⚠️ SMTP credentials are placeholders (expected)
- ⚠️ Currently using stub `EmailService` (emails logged, not sent)

---

## Production Setup Instructions

### Step 1: Choose SMTP Provider

Select one of:
- Gmail (with App Password)
- Brevo (Sendinblue)
- Microsoft 365 / Outlook
- Custom SMTP server

### Step 2: Configure Environment Variables

Set the following environment variables (recommended for production):

```bash
Email__Provider=SMTP
Email__SmtpHost=smtp.your-provider.com
Email__SmtpPort=587
Email__SmtpUsername=your-username
Email__SmtpPassword=your-password
Email__FromEmail=noreply@yourdomain.com
Email__FromName=IntelliPM
Email__EnableSsl=true
```

### Step 3: Verify Configuration

1. Start the application
2. Check logs for email service registration:
   - Should see: `SmtpEmailService` registered (if credentials set)
   - Or: `EmailService` (stub) registered (if credentials not set)
3. Test email sending via admin panel or health check endpoint:
   - `GET /api/health/smtp` (development only)
   - `POST /api/health/smtp/send-test` (development only)

### Step 4: Verify Email Delivery

- Send a test email
- Check recipient inbox
- Verify email appears (not in spam)
- Check application logs for success/error messages

---

## Security Considerations

### ⚠️ Sensitive Information

**DO NOT commit the following to version control:**
- `Email:SmtpPassword` - Use environment variables or user secrets
- `Email:SmtpUsername` - Can be in config if not sensitive, but prefer env vars

### Best Practices

1. **Use environment variables** for production credentials
2. **Use User Secrets** for local development
3. **Never commit** `.env` files with real credentials
4. **Rotate passwords** regularly
5. **Use App Passwords** for Gmail (not regular passwords)
6. **Verify sender addresses** in provider dashboard (Brevo, etc.)

---

## Troubleshooting

### Emails Not Sending

1. **Check DI registration:**
   - Verify `Email:Provider == "SMTP"`
   - Verify `Email:SmtpUsername` is not empty/placeholder
   - Check application startup logs

2. **Check SMTP credentials:**
   - Verify username and password are correct
   - For Gmail: Use App Password, not regular password
   - For Brevo: Verify sender email is verified in dashboard

3. **Check SMTP connection:**
   - Use health check endpoint: `GET /api/health/smtp` (dev only)
   - Check firewall/network connectivity
   - Verify port is not blocked

4. **Check logs:**
   - Look for SMTP connection errors
   - Check for authentication failures
   - Verify email sending attempts

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Emails logged but not sent | Using stub `EmailService` | Set `Email:SmtpUsername` to non-empty value |
| Authentication failed | Wrong credentials | Verify username/password |
| Connection timeout | Firewall blocking port | Allow outbound port 587/465 |
| Emails marked as spam | Unverified sender | Verify `FromEmail` in provider dashboard |
| Gmail rejects emails | FromEmail != SmtpUsername | Set `FromEmail` to match `SmtpUsername` |

---

## Files Referenced

- ✅ `backend/IntelliPM.API/appsettings.json` (lines 87-96)
- ✅ `backend/IntelliPM.Infrastructure/DependencyInjection.cs` (lines 98-106)
- ✅ `backend/IntelliPM.Infrastructure/Services/SmtpEmailService.cs`
- ✅ `backend/IntelliPM.Infrastructure/Services/EmailService.cs` (stub)
- ✅ `backend/IntelliPM.API/Controllers/HealthController.cs` (SMTP test endpoints)

---

## Conclusion

✅ **Configuration structure is correct**  
✅ **DI registration logic is correct**  
✅ **All required keys are present**  
⚠️ **SMTP credentials need to be configured for production** (currently placeholders)

**Next Steps:**
1. Set `Email:SmtpUsername` and `Email:SmtpPassword` via environment variables
2. Verify `Email:Provider == "SMTP"` in production
3. Test email sending via health check endpoints
4. Monitor logs for email delivery success/failure

---

**End of Report**

