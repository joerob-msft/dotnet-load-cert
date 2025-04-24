# Azure App Service Certificate Inventory (.NET 8)

![Azure](https://img.shields.io/badge/azure-%230072C6.svg?style=for-the-badge&logo=microsoftazure&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)

A lightweight ASP.NET Core web application that provides a human-readable inventory of TLS/SSL certificates in your Azure App Service Windows environment.

## Features

- Lists public certificates from Windows certificate stores (CA and Root)
- Lists private/personal certificates from Windows certificate stores (My and WebHosting)
- Displays certificate metadata including:
  - Subject and issuer information
  - Validity dates and expiration status
  - Serial numbers and thumbprints
  - Private key availability
- Shows environment variables related to certificates
- Clean, responsive Bootstrap UI

## Screenshot
![image](https://github.com/user-attachments/assets/5ef0925e-9ec5-425d-9ed6-a0abe288925d)

## Deployment

### Prerequisites

- An Azure App Service (Windows) with .NET 8 runtime
- GitHub account to host the repository
- Access to GitHub Actions or Azure DevOps pipelines

### Deployment Methods

#### 1. GitHub Actions (Recommended)

1. Fork or clone this repository to your GitHub account
2. In your GitHub repository settings, add these secrets:
   - `AZURE_WEBAPP_NAME`: Your App Service name
   - `AZURE_WEBAPP_PUBLISH_PROFILE`: Your publish profile XML content (download from Azure Portal)
3. Push to the main branch, and GitHub Actions will deploy to your Azure App Service

#### 2. Manual Deployment via Azure Portal

1. Clone this repository to your local machine
2. In the Azure Portal, navigate to your App Service
3. Go to Deployment Center
4. Choose your preferred deployment method (Local Git, GitHub, etc.)
5. Follow the prompts to connect your repository
6. Deploy the code to your App Service

#### 3. Visual Studio Deployment

1. Open the solution in Visual Studio
2. Right-click on the project in Solution Explorer
3. Select "Publish..."
4. Follow the wizard to publish to your Azure App Service

## Required App Service Configuration

Ensure your App Service has these configurations:

1. **Platform**: Windows
2. **Runtime Stack**: .NET 8 (LTS)
3. **Application Settings**:
   - `WEBSITE_LOAD_CERTIFICATES`: Set to `*` to load all certificates or specific thumbprints separated by commas

## Connecting to the Deployments Blade

To monitor and manage deployments through the Azure Portal:

1. Navigate to your App Service in the Azure Portal
2. Select "Deployment Center" from the left navigation menu
3. If using GitHub Actions, you'll see your deployments listed here
4. Select a deployment to view logs and status details
5. Use "Deployment Logs" to troubleshoot any deployment issues

## Certificate Access Requirements

For the application to access certificates:

1. The application needs read access to Windows certificate stores
2. Ensure `WEBSITE_LOAD_CERTIFICATES` is set to `*` to load all certificates or specific thumbprints to load individual certificates
3. If managed certificates are used, these are automatically available in the appropriate stores

## How Certificates are Stored in Windows App Service

Unlike Linux App Services where certificates are stored as files, Windows App Services store certificates in the Windows Certificate Store:

- Public certificates (CA, root certificates) are stored in the `LocalMachine\CertificateAuthority` and `LocalMachine\Root` stores
- Private certificates (with private keys) are stored in the `LocalMachine\My` store
- App Service specific certificates may also be in the `LocalMachine\WebHosting` store

## Development

### Local Development

```bash
# Clone repository
git clone https://github.com/yourusername/azure-cert-inventory-dotnet.git
cd azure-cert-inventory-dotnet

# Build and run
dotnet build
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.

### Requirements

- .NET 8 SDK
- Visual Studio 2022 or Visual Studio Code (optional)

## License

[MIT License](LICENSE)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
