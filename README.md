# README.md for Secure Software Development Project: OAuth 2.0 and OpenID Connect
![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C%23](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white)
![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black)
![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Keycloak](https://img.shields.io/badge/Keycloak-B3472D?style=for-the-badge&logo=keycloak&logoColor=white)

## Project Overview
This project is an implementation of OAuth 2.0 and OpenID Connect client protocol with PKCE (Proof Key for Code Exchange) using ASP.NET Core. The primary goal is to deepen understanding of the protocol by implementing it from scratch without using dedicated OAuth libraries. This approach enhances troubleshooting skills and aids in grasping when and how the protocol can be securely used in production environments.

## Features
- **OAuth 2.0 Authorization Code Flow with PKCE**: Ensures secure authentication by exchanging the authorization code for access, refresh, and ID tokens.
- **Dynamic Configuration**: Utilizes configuration settings from `appsettings.json` to manage Keycloak endpoints, enhancing flexibility and security.
- **Token Validation**: Includes ID token verification to ensure the authenticity and integrity of the tokens received from the authorization server.

## Technology Stack
- **.NET 8.0 MVC**: Used for server-side handling of the OAuth flow and user sessions.
- **Keycloak**: As the OpenID Connect provider to authenticate and authorize users.
- **Docker**: For running the Keycloak server locally.
- **C#**: Main programming language.

## Setup and Installation
### Prerequisites
- .NET SDK
- Docker
- Any IDE that supports .NET development (e.g., Visual Studio, VS Code)

### Running Keycloak Server
1. Start Keycloak using Docker:
   ```bash
   docker run -p 8080:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin quay.io/keycloak/keycloak:21.1.0 start-dev
   ```
2. Access the Keycloak admin console at http://localhost:8080/admin/ and login using the admin credentials.

### Configure Keycloak
1. Under “Clients” in the sidebar, click “Create Client”.
2. Set Client Type to "OpenID Connect" and fill in the necessary details like Client ID.
3. Ensure that “Client authentication” is enabled.
4. Add valid redirect URIs (e.g., http://localhost:5000/callback).

## Running the Application
1. From the terminal or command prompt, navigate to the project directory:
```bash
dotnet run
```
2. Open a web browser and navigate to http://localhost:5000/ to access the application.

## Usage

* Click on the login link to authenticate using Keycloak.
* After authentication, the user is redirected back to the application where the tokens are exchanged, and user information is fetched and displayed.

## Security Measures

* All communication with the Keycloak server should be over HTTPS in production environments.
* Store sensitive information such as client secrets securely using environment variables or secure vault solutions.

## Documentation

* Detailed API documentation for Keycloak can be found here.
* For more information on implementing OAuth 2.0 and OpenID Connect, refer to the official OAuth 2.0 documentation.

## Contributing

Contributions to this project are welcome. Please fork the repository, make your changes, and submit a pull request.

## License

This project is open-source and available under the MIT License.
