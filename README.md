# Charter of Turin Monitor
Tasklist application to replace the default 'Camunda Tasklist' with the added functionalities of providing an innovative interface to interact with the BPMN diagram itself to approve tasks, upload evidence, and communicating with a third party hosted on AWS to retrieve activity sensor predictions.

## Configuration

### Setup Environment Variables

All secrets and configuration are managed through environment variables for security. Follow these steps:

#### 1. Create .env file (Local Development)

Copy the `.env.example` file to `.env` and fill in with the desired values.

### Edit file mongo-init.js  (TODO to be fixed/removed)

Change the AWS credentials and the credentials for the first admin user.

#### 2. Docker Deployment

For Docker Compose deployments, create a `.env` file in the project root with all required environment variables. Docker Compose will automatically load and pass these to the container.
