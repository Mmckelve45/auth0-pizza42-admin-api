# Pizza42 Admin API - TODO & Progress

## ✅ Completed

### Project Setup
- [x] Created .NET 8 Web API with Minimal API pattern
- [x] Connected to PostgreSQL on Neon (Vercel)
- [x] Created Pizza and Order models matching existing DB schema
- [x] Set up Entity Framework Core with Npgsql

### API Endpoints
- [x] **GET** `/api/pizzas` - Get all pizzas (requires auth)
- [x] **GET** `/api/pizzas/{id}` - Get specific pizza (requires auth)
- [x] **PUT** `/api/pizzas/{id}/price` - Update pizza price (Employee only)
- [x] **PUT** `/api/pizzas/{id}/soldout` - Toggle sold out status (Employee only)
- [x] **GET** `/api/orders` - Get 25 most recent orders, sorted newest first (Employee only)
- [x] **GET** `/api/orders/{id}` - Get specific order (Employee only)
- [x] **PUT** `/api/orders/{id}/status` - Update order status (Employee only)
- [x] **PUT** `/api/orders/{id}/priority` - Update order priority (Employee only)

### Security & Auth
- [x] Configured Auth0 JWT authentication
  - Domain: `pizza42-mckelvey.us.auth0.com`
  - Audience: `https://pizza42/api`
  - Role claim: `https://pizza42.com/role` (singular, not plural!)
- [x] Created "EmployeeOnly" authorization policy
- [x] Protected endpoints appropriately:
  - GET pizzas: Any authenticated user
  - PUT operations: Employee role required
  - GET orders: Employee role required
- [x] Added CORS for React app (ports 3000, 3002, 5173)
- [x] Swagger UI with JWT authentication support

### Configuration & Git
- [x] Separated sensitive credentials into `appsettings.Development.json` (gitignored)
- [x] Created .gitignore for .NET projects
- [x] Ready for public GitHub repo

### Fixed Issues
- [x] Fixed EF Core version compatibility (downgraded to 8.0.11 for .NET 8)
- [x] Fixed duplicate Column attribute on Pizza.UnitPrice
- [x] Fixed connection string format for Npgsql
- [x] Fixed 403 error - changed role claim from "roles" (plural) to "role" (singular)

---

## 🎯 IIS Deployment (COMPLETED!)

### Step 1: Prepare for Production ✅
- [x] Updated `appsettings.json` with production settings
- [x] Configured production connection string directly in appsettings.json
- [x] Built for production: `dotnet build --configuration Release`
- [x] Published app: `dotnet publish --configuration Release --output ./publish`

### Step 2: IIS Setup ✅
- [x] IIS already installed on Windows Server
- [x] Installed ASP.NET Core Hosting Bundle 8.0.11 for .NET 8
  - Downloaded from: https://dotnet.microsoft.com/download/dotnet/8.0
  - Restarted IIS: `iisreset`
- [x] Created IIS Application Pool
  - .NET CLR Version: "No Managed Code" (for .NET Core!)
  - Name: `Pizza42AdminApiPool`
- [x] Created IIS Website
  - Physical Path: `C:\inetpub\pizza42-api`
  - Application Pool: `Pizza42AdminApiPool`
  - Binding: `http://*:8080`

### Step 3: Configuration ✅
- [x] Enabled Swagger in production (for internal admin API)
- [x] Configured database connection string in `appsettings.json`
- [x] Verified Auth0 JWT authentication works
- [x] Tested all endpoints via Swagger UI

### Current Production Setup
- **Direct API URL (DuckDNS)**: `https://mckelvey-server.duckdns.org` or `http://mckelvey-server.duckdns.org:8080`
- **Public Gateway URL**: `https://jni2138npa.execute-api.us-east-1.amazonaws.com`
- **Swagger UI**: `http://localhost:8080/swagger` (internal only)
- **Status**: ✅ Fully operational with AWS Gateway integration!

### Step 4: SSL & External Access ✅
- [x] Installed self-signed SSL certificate for IIS (mckelvey-server.duckdns.org)
- [x] Configured HTTPS binding in IIS (port 443)
- [x] Set up DuckDNS dynamic DNS (mckelvey-server.duckdns.org)
- [x] Created scheduled task to update DuckDNS IP every 5 minutes
- [x] Configured Windows Firewall rules for inbound HTTPS (443) and HTTP (8080)
- [x] Set up router port forwarding (443 → 192.168.0.50:443, 8080 → 192.168.0.50:8080)
- [x] Tested external access successfully via DuckDNS
- [x] Updated CORS origins to include production Vercel app (`https://auth0-pizza42.vercel.app`)

### Step 5: Conditional HTTPS Redirection ✅
- [x] Implemented port-based HTTPS redirection middleware
  - Port 8080 (backend): Allows HTTP for AWS Gateway
  - Other ports: Redirects HTTP to HTTPS for direct access
  - Provides security for direct access while allowing Gateway's HTTP backend

---

## 🚀 AWS API Gateway Setup (COMPLETED!)

### Prerequisites ✅
- [x] IIS deployment working and accessible
- [x] Dynamic DNS configured (DuckDNS)
- [x] Firewall configured for external access

### Gateway Setup ✅
- [x] Created AWS HTTP API Gateway (API ID: `jni2138npa`)
- [x] Configured JWT authorizer for Auth0 validation
  - Issuer URL: `https://pizza42-mckelvey.us.auth0.com/`
  - Audience: `https://pizza42/api`
  - Identity source: `$request.header.Authorization`
- [x] Set up HTTP proxy integration to on-prem server
  - Integration URI: `http://mckelvey-server.duckdns.org:8080/{proxy}`
  - Integration ID: `okdul3r`
- [x] Configured catch-all route: `ANY /{proxy+}`
- [x] Created separate OPTIONS route to bypass auth (for CORS preflight)
- [x] Deployed API to $default stage (auto-deploy enabled)
- [x] Configured CORS for localhost dev and production Vercel app
  - Allowed origins: localhost:3000, 3002, 5173, https://auth0-pizza42.vercel.app
  - Allowed methods: GET, POST, PUT, DELETE, OPTIONS
  - Allowed headers: Content-Type, Authorization
- [x] Tested end-to-end from React app successfully

### Production Endpoints
**Public Gateway URL**: `https://jni2138npa.execute-api.us-east-1.amazonaws.com`

All endpoints available at Gateway:
- `GET /api/pizzas` - Get all pizzas (any authenticated user)
- `GET /api/pizzas/{id}` - Get specific pizza (any authenticated user)
- `PUT /api/pizzas/{id}/price` - Update pizza price (Employee only)
- `PUT /api/pizzas/{id}/soldout` - Toggle sold out status (Employee only)
- `GET /api/orders` - Get 25 most recent orders (Employee only)
- `GET /api/orders/{id}` - Get specific order (Employee only)
- `PUT /api/orders/{id}/status` - Update order status (Employee only)
- `PUT /api/orders/{id}/priority` - Update order priority (Employee only)

### Cost Analysis (Actual)
- **HTTP API Gateway**: ~$1/month for 1 million requests
- **Current usage**: Essentially free (well under free tier limits)
- **Free tier**: 1 million API calls/month for first 12 months
- **No data transfer charges** for on-prem backend integration

---

## 🔮 Future Enhancements

### CloudWatch Logging & Monitoring
- [ ] Set up CloudWatch Log Group for API Gateway
- [ ] Enable access logging and execution logging
- [ ] Configure alerts for high error rates or latency

### Advanced Authorization
- [ ] Experiment with Lambda authorizer for custom auth logic (LEARNING TASK)
- [ ] Add role-based access control for additional roles (e.g., Manager, Admin)

### Performance & Reliability
- [ ] Set up API Gateway throttling and rate limiting
- [ ] Configure API Gateway caching for GET requests
- [ ] Add health check endpoint for monitoring

### SSL Certificate Upgrade
- [ ] Consider upgrading from self-signed to Let's Encrypt certificate
- [ ] Or purchase commercial SSL certificate for production use
- [ ] Switch Gateway integration from HTTP to HTTPS if cert upgraded

---

## 📝 Important Configuration Details

### Auth0 Setup
- **Domain**: `pizza42-mckelvey.us.auth0.com`
- **Audience**: `https://pizza42/api`
- **Role Claim Name**: `https://pizza42.com/role` (SINGULAR!)
- **Role Value**: `Employee`
- Auth0 Action adds role to access token (external system integration)

### Database
- **Host**: `ep-mute-shape-adj3tccp-pooler.c-2.us-east-1.aws.neon.tech`
- **Database**: `neondb`
- Connection string in `appsettings.Development.json` (gitignored)
- Supports connections from anywhere (Neon serverless)

### Dynamic DNS & External Access
- **DuckDNS Domain**: `mckelvey-server.duckdns.org`
- **DuckDNS Token**: (stored in scheduled task)
- **Update Frequency**: Every 5 minutes via Windows Task Scheduler
- **Ports**:
  - 443 (HTTPS): Direct access with self-signed cert
  - 8080 (HTTP): AWS Gateway backend access
- **Router**: Cox (port forwarding configured)
- **Server IP**: 192.168.0.50 (static DHCP reservation)

### Current Development Setup
- **Local API runs on**: `http://localhost:5041` (HTTPS redirect enabled)
- **Local Swagger UI**: `http://localhost:5041/swagger`
- **React dev servers**: ports 3000, 3002, 5173 (CORS enabled)

### Production Setup
- **Public Gateway URL**: `https://jni2138npa.execute-api.us-east-1.amazonaws.com`
- **Direct DuckDNS HTTPS**: `https://mckelvey-server.duckdns.org`
- **Direct DuckDNS HTTP (backend)**: `http://mckelvey-server.duckdns.org:8080`
- **Production React App**: `https://auth0-pizza42.vercel.app` (CORS enabled)

### Project Structure
```
auth0-pizza42-admin-api/
├── Models/
│   ├── Pizza.cs          # Pizza entity (id, name, unit_price, sold_out, created_at, updated_at)
│   └── Order.cs          # Order entity (id, user_id, order_data, status, priority, created_at, updated_at)
├── Data/
│   └── PizzaDbContext.cs # EF Core DbContext
├── Program.cs            # Main app configuration (auth, endpoints, middleware)
├── appsettings.json      # Public config (safe for GitHub)
├── appsettings.Development.json  # Private config with real DB credentials (GITIGNORED)
├── PizzaAdminApi.csproj  # Project file with NuGet packages
└── README.md             # Documentation

Key Dependencies:
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11
- Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4
- Microsoft.EntityFrameworkCore.Design 9.0.10
- Swashbuckle.AspNetCore 6.6.2
```

---

## 🔍 Quick Start Commands

```bash
# Run in development
dotnet run

# Run with hot reload
dotnet watch run

# Build for production
dotnet build --configuration Release

# Publish for deployment
dotnet publish --configuration Release --output ./publish

# Test endpoints
curl -X GET http://localhost:5041/api/pizzas
```

---

## 📚 Learning Resources

- [Deploy ASP.NET Core to IIS](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/)
- [AWS API Gateway + On-Prem Integration](https://aws.amazon.com/blogs/compute/introducing-amazon-api-gateway-private-integration/)
- [Auth0 ASP.NET Core Quickstart](https://auth0.com/docs/quickstart/backend/aspnet-core-webapi)

---

## 🐛 Known Issues & Solutions

### Issue: 403 Forbidden with valid token
**Solution**: Check that role claim is `https://pizza42.com/role` (singular), not `roles` (plural)

### Issue: Can't connect to database from IIS
**Solution**:
1. Ensure connection string uses environment variable or Key Vault
2. Check IIS Application Pool identity has network access
3. Verify Neon allows connections from server IP

### Issue: CORS errors from React frontend when calling Gateway
**Root Cause**: Browser sends OPTIONS preflight request, which was hitting the JWT authorizer and getting rejected (401). The 401 response had no CORS headers, so browser showed "CORS error" instead of auth error.

**Solution**:
1. Create separate OPTIONS route that bypasses the JWT authorizer
2. Configure CORS on API Gateway (AllowOrigins, AllowMethods, AllowHeaders)

```bash
# Create OPTIONS route without authorizer
aws apigatewayv2 create-route \
  --api-id jni2138npa \
  --route-key 'OPTIONS /{proxy+}' \
  --target 'integrations/okdul3r' \
  --region us-east-1
```

### Issue: AWS Gateway 302 redirect to HTTPS on port 8080
**Root Cause**: IIS was redirecting all HTTP requests to HTTPS, including Gateway backend requests.

**Solution**: Implemented port-based conditional HTTPS redirection middleware:
- Port 8080: Always allows HTTP (for AWS Gateway backend)
- Other ports: Redirects HTTP to HTTPS (for direct access security)

See `Program.cs:92-114` for implementation.

### Issue: AWS Gateway 500 error with self-signed certificate
**Root Cause**: AWS Gateway cannot verify self-signed SSL certificates when connecting to backend.

**Solution**: Changed Gateway integration from HTTPS to HTTP (port 8080), avoiding SSL verification entirely while maintaining Gateway → Browser HTTPS security.

---

## 💡 Architecture Decisions

### Why same Auth0 API instead of creating new one?
- Reusing existing API with additional scopes is simpler
- Single audience for all related services
- No need to manage multiple API configurations

### Why require auth for GET pizzas (not anonymous)?
- This is an admin API, not a customer-facing menu API
- Keeps it secure and controlled
- Any logged-in user can view, only Employees can modify

### Why defense-in-depth (Gateway + API auth)?
- Gateway validates tokens before reaching on-prem server
- API validates again as backup
- Reduces attack surface
- Gateway can handle rate limiting and DDoS protection

### Why HTTP backend (port 8080) instead of HTTPS?
- AWS Gateway cannot verify self-signed SSL certificates
- Using HTTP for Gateway → Server communication avoids SSL verification issues
- Gateway → Browser is still HTTPS (secure public endpoint)
- Conditional middleware forces direct HTTPS access for security
- No additional security risk since traffic goes through Gateway's HTTPS

### Why separate OPTIONS route?
- Browser sends OPTIONS preflight before actual requests (CORS)
- Preflight requests don't include Authorization header
- JWT authorizer would reject OPTIONS requests (401)
- Separate OPTIONS route bypasses authorizer, allowing preflight to succeed
- Actual GET/POST/PUT requests still go through JWT validation

---

**Last Updated**: 2025-10-29
**Status**: ✅ **PRODUCTION READY!** API deployed to IIS, AWS Gateway configured with JWT auth, CORS working, tested end-to-end from React app!
