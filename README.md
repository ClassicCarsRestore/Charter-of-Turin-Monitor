# Charter of Turin Monitor
Tasklist application to replace the default 'Camunda Tasklist' with the added functionalities of providing an innovative interface to interact with the BPMN diagram itself to approve tasks, upload evidence, and communicating with a third party hosted on AWS to retrieve activity sensor predictions.

## Configuration

### Edit file Settings.cs

Change the secrets, the email credentials, and the Pinterest app information.

Format:

```bash
public const string Secret = "YourSecret";

public const string Camera_Hub_Secret = "CameraHubSecret";

public const string Email_Address = "EmailAddress";
public const string Email_Password = "EmailPassword";

public const string Pinterest_ID = "PinterestAppID";
public const string Pinterest_Secret = "PinterestAppSecret";
public const string Pinterest_Account = "PinterestAccountUsername";
```

### Edit file mongo-init.js

Change the AWS credentials and the credentials for the first admin user.

Format:

```bash
db = db.getSiblingDB('TasklistDb');

db.createCollection('Credentials');

db.Credentials.insertOne(
	{
		username: "s3-processed-data-read-access-user",
		access_key_id: "your_access_key_id",
		secret_access_key: "your_secret_access_key"
	}
);

db.createCollection('LoginCredentials');

db.LoginCredentials.insertOne(
	{
		role: "admin",
		email: "AdminEmail",
		password: "AdminPassword"
	}
);
```
