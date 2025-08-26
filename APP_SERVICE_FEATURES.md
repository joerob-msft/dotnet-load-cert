# App Service API Features

This document describes the App Service optimized certificate management features.

## 🚀 App Service Optimized Features

### **Automatic Service Selection**
The application automatically detects when running in App Service and uses the optimized `AppServiceCertificateService` instead of the basic `CertificateService`.

### **New API Endpoints**

#### **Certificate Management**
- `GET /api/cert-inventory/certificates/appservice` - Get certificates loaded via `WEBSITE_LOAD_CERTIFICATES`
- `GET /api/cert-inventory/certificates/loaded` - Get certificates in application memory
- `POST /api/cert-inventory/certificates/load` - Load certificate into memory
- `POST /api/cert-inventory/certificates/validate` - Validate certificate with comprehensive checks
- `DELETE /api/cert-inventory/certificates/loaded/{thumbprint}` - Remove certificate from memory

#### **Enhanced Features**
- ✅ **In-Memory Certificate Storage** - Compatible with App Service restrictions
- ✅ **App Service Certificate Detection** - Automatically finds certificates loaded via environment variables
- ✅ **Certificate Validation** - Chain validation, expiration checks, revocation status
- ✅ **Base64 Certificate Import** - Load certificates from encoded data
- ✅ **Memory Management** - Add/remove certificates without touching system stores

## 📋 API Examples

### Load Certificate
```json
POST /api/cert-inventory/certificates/load
{
  "certificateData": "MIIFjTCCA3WgAwIBAgIQ...",
  "password": "optional-password",
  "friendlyName": "My SSL Certificate"
}
```

### Validate Certificate
```json
POST /api/cert-inventory/certificates/validate
{
  "thumbprint": "ABCDEF1234567890ABCDEF1234567890ABCDEF12",
  "validateChain": true,
  "checkRevocation": false
}
```

## 🔧 App Service Configuration

### Environment Variables
- `WEBSITE_LOAD_CERTIFICATES=*` - Load all uploaded certificates
- `WEBSITE_LOAD_CERTIFICATES=thumbprint1,thumbprint2` - Load specific certificates

### Certificate Upload
1. Upload certificates via Azure Portal → App Service → TLS/SSL Settings
2. Set `WEBSITE_LOAD_CERTIFICATES` environment variable
3. Restart the application
4. Certificates will be available via `/api/cert-inventory/certificates/appservice`

## 🛡️ Security Features

- **Read-Only Access** - No modification of system certificate stores
- **Memory Isolation** - Loaded certificates are isolated to application instance
- **Automatic Cleanup** - Certificates are disposed when removed from memory
- **App Service Optimized** - Works within App Service security constraints

## 🚦 Route Protection

The application includes middleware to protect existing routes:
- `/ai/*` - Protected for site extensions
- `/.well-known/*` - Protected for ACME/well-known endpoints

All certificate inventory APIs are under `/api/cert-inventory/` to avoid conflicts.
