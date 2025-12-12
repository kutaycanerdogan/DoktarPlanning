
#### Doktar Planning API

Doktar Planning API is a task and reminder management system built with .NET, Hangfire, SQL Server, Redis, and Docker. It supports recurring tasks, scheduled reminders, email and webhook notifications, and background job processing. This document explains how to run the project locally, how the environment configuration works, and how to test the system end to end.

The project can run in two local environments: Windows or Docker. Shared configuration values are stored in appsettings.json. Environment-specific overrides are stored in appsettings.Development.json and appsettings.Docker.json. Only values that differ between environments should appear in these files.

Running on Windows: When running the API directly through Visual Studio or using “dotnet run”, the application loads appsettings.Development.json. You must configure your own SQL Server connection string. SMTP settings must also be configured if you want to send real emails, for example using Gmail. Azure Key Vault is optional; if you provide a VaultUri, Key Vault integration becomes active automatically. HTTPS can be enabled by uncommenting the Kestrel configuration. Redis is defined but not used in the Windows environment.

Running on Docker: When running through Docker Compose, the application loads appsettings.Docker.json. SQL Server, Redis, MailHog, and Seq are already configured and start automatically. You normally do not need to change anything. If you change ports or passwords, update both appsettings.Docker.json and docker-compose.yml.

Useful URLs:
- Swagger: [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)
- Hangfire Dashboard: [http://localhost:5000/hangfire](http://localhost:5000/hangfire)
- MailHog: [http://localhost:8025](http://localhost:8025)
- Seq: [http://localhost:5341](http://localhost:5341)
- Webhook testing: [https://webhook.site](https://webhook.site)

## Recommended workflow for testing:

1. Open Swagger.

2. If you do not have an account, call the signup endpoint. If you do, call signin.

3. Copy the returned JWT token and use the Authorize button in Swagger to authenticate.

4. Create a recurrence rule. Frequency must be 0 (Daily). Interval defines how many days between occurrences. Use a small number like 1 while testing. OccurrenceCount must be greater than zero.

5. Create a new task. Task names must be unique. Leave reminder fields empty.

6. If you want a recurring task, leave recurrenceRuleId empty.

7. Optionally add subtasks or notes.

8. Create a reminder. Channel 0 means email, channel 1 means webhook. ChannelTarget should be an email address or a webhook URL. [https://webhook.site](https://webhook.site) provides a temporary URL you can use.

9. If you do not want to wait for the scheduled time, you can manually trigger a reminder using the endpoint: POST /api/Reminders/tasks/{taskId}/reminder/send

10. Test the channel you used to confirm that the reminder was delivered.

If reminders do not fire, the most common causes are timezone differences (the system uses UTC and Turkey Time is UTC+3), SMTP configuration issues, or webhook endpoint problems.

Enum values:

    Priority
      {
          Low = 0,
          Medium = 1,
          High = 2
      }
  
    Frequency
      {
          Daily = 0
      }
  
    ReminderChannel
      {
          Email = 0,
          Webhook = 1
      }

CI/CD: The project includes two GitHub Actions workflows. test.yml runs unit tests and publishes test results. publish.yml builds and publishes artifacts. There is no deployment pipeline at the moment.

### Configuration Files
# appsettings.json, appsettings.Development.json

MailHog  =>

	"Smtp": {
	"Host": "localhost",
	"Port": "1025",
	"User": "",
	"Password": "",
	"From": "noreply@local",
	"EnableSsl": false
	}
 
 Redis  => 
 
	"Redis": {
	  "Configuration": "localhost:6379"
	},
 
 Seq  => 
 
    "Serilog": {
      "WriteTo": [
        {
          "Name": "Seq",
          "Args": { "serverUrl": "http://localhost:5341" }
        }
      ]
    },
 
# appsettings.Docker.json  
 
 MailHog  =>
 
    "Smtp": {
       "Host": "mailhog",
       "Port": "1025",
       "User": "",
       "Password": "",
       "From": "noreply@local",
       "EnableSsl": false
    }
 
 Redis  =>
 
    "Redis": {
      "Configuration": "redis:6380"
    },

 MSSql  => 
 
    "ConnectionStrings": {
      "DefaultConnection": "Server=sqlserver,1433;Database=DoktarPlanning;User Id=sa;Password=Your_strong!Passw0rd;"
    },
  
  Seq  =>
  
    "Serilog": {
      "WriteTo": [
        {
          "Name": "Seq",
          "Args": { "serverUrl": "http://seq:5341" }
        }
      ]
    },

If you change any ports or passwords, make sure to update docker-compose.yml accordingly.