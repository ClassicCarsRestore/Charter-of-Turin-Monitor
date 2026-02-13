using System;

namespace tasklist
{
    public static class Settings
    {
        private static string GetRequiredEnvVar(string varName)
        {
            var value = Environment.GetEnvironmentVariable(varName);
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Required environment variable '{varName}' is not set. Please configure it in .env file or system environment variables.");
            }
            return value;
        }

        private static string GetEnvVar(string varName, string defaultValue = null)
        {
            return Environment.GetEnvironmentVariable(varName) ?? defaultValue;
        }

        public static string Platform_URL => GetRequiredEnvVar("PLATFORM_URL");
        public static string Blockchain_API_URL => GetEnvVar("BLOCKCHAIN_API_URL");

        public static string Inventory_API_URL => GetEnvVar("INVENTORY_API_URL");

        // Authentication
        public static string Salt => GetRequiredEnvVar("PASSWORD_SALT");

        // Authentik Configuration
        public static string AuthentikTokenUrl => GetRequiredEnvVar("AUTHENTIK_TOKEN_URL");
        public static string Authentik_Client_ID => GetRequiredEnvVar("AUTHENTIK_CLIENT_ID");
        public static string Authentik_Client_Secret => GetRequiredEnvVar("AUTHENTIK_CLIENT_SECRET");

        // API Secrets
        public static string Camera_Hub_Secret => GetRequiredEnvVar("CAMERA_HUB_SECRET");
        public static string Connector_Secret => GetRequiredEnvVar("CONNECTOR_SECRET");

        // Email Configuration
        public static string SMTP_Host => GetEnvVar("SMTP_HOST");
        public static int SMTP_Port => int.Parse(GetEnvVar("SMTP_PORT", "587"));

        public static string Email_Address => GetRequiredEnvVar("EMAIL_ADDRESS");
        public static string Email_Password => GetRequiredEnvVar("EMAIL_PASSWORD");

        // Pinterest OAuth Configuration
        public static string Pinterest_ID => GetRequiredEnvVar("PINTEREST_ID");
        public static string Pinterest_Secret => GetRequiredEnvVar("PINTEREST_SECRET");
        public static string Pinterest_Redirect_URI => GetRequiredEnvVar("PINTEREST_REDIRECT_URI");
        public static string Pinterest_Scope => GetRequiredEnvVar("PINTEREST_SCOPE");
        public static string Pinterest_API_URL => GetRequiredEnvVar("PINTEREST_API_URL");
        public static string Pinterest_Photos_URL => GetRequiredEnvVar("PINTEREST_PHOTOS_URL");
        public static string Pinterest_Account => GetRequiredEnvVar("PINTEREST_ACCOUNT");
        public static string Pinterest_Cover_Image_URL => GetEnvVar("PINTEREST_COVER_IMAGE_URL");

        // Camunda Configuration
        public static string Camunda_Base_URL => GetRequiredEnvVar("CAMUNDA_BASE_URL");
    }
}
