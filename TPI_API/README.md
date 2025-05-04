## Running the Project with Docker

This project is containerized using Docker and can be run easily with Docker Compose. Below are the project-specific instructions and requirements for running the application in a Docker environment.

### Requirements
- **.NET Version:** 8.0 (as specified in the Dockerfile)
- **Native Binaries:** The container includes native binaries (e.g., `pdfium.dll`) and the `tessdata` directory for OCR functionality.

### Environment Variables
- The application supports environment variables via a `.env` file. An example file is provided as `.env.example`.
- **Note:** Uncomment the `env_file: ./.env` line in `docker-compose.yml` if you wish to use environment variables from a `.env` file.

### Build and Run Instructions
1. **(Optional) Configure Environment Variables:**
   - Copy `.env.example` to `.env` and adjust values as needed.
2. **Build and Start the Application:**
   - Run the following command in the project root:
     ```sh
     docker compose up --build
     ```
   - This will build the Docker image and start the service defined as `csharp-app`.

### Special Configuration
- The Dockerfile copies the `NativeBinaries` and `tessdata` directories into the container and sets permissions for a non-root user (`appuser`).
- If you need to add external services (e.g., a database), update the `depends_on` section in `docker-compose.yml`.

### Exposed Ports
- **csharp-app:**
  - Exposes port **80** (default ASP.NET port)
  - Accessible at `http://localhost:80` after running the container

---

*Ensure Docker and Docker Compose are installed on your system before running the above commands.*