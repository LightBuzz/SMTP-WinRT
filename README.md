# SMTP client for Windows Store

An SMTP client for Windows Store apps (WinRT and UWP). Send emails from within your Windows Universal App.

## Installation
You can download and build this project or simply install it via NuGet:
	
	PM> Install-Package lightbuzz-smtp

This requires the 'internetClient' Capability to be enabled in the Package.appxmanifest of the universal app.
## Examples
Import the assembly to your project and include its namespace:
	
	using LightBuzz.SMTP;

### Send an email message
*This is an example of using your own SMTP server. Check below for using Gmail and Outlook.*
  
  	using (SmtpClient client = new SmtpClient("smtp.example.com", 465, false, "info@example.com", "Pa$$w0rd"))
  	{
	        EmailMessage emailMessage = new EmailMessage();
	
	        emailMessage.To.Add(new EmailRecipient("someone1@anotherdomain.com"));
	        emailMessage.CC.Add(new EmailRecipient("someone2@anotherdomain.com"));
	        emailMessage.Bcc.Add(new EmailRecipient("someone3@anotherdomain.com"));
	        emailMessage.Subject = "Subject line of your message";
	        emailMessage.Body = "This is an email sent from a WinRT app!";
	        
	        await client.SendMailAsync(emailMessage);
  	}
  
### Credentials for Gmail

	Server: smtp.gmail.com
  	Port: 465
  	SSL: True
  
#### Important Note for Gmail

Since this does not use OAUTH2, Gmail considers this a "less secure app".  To use this with Gmail, the "Access for less secure apps" setting on the account will have to be changed to "Enable".
  
### Credentials for Outlook

	Server: smtp-mail.outlook.com
  	Port: 587
  	SSL: False (upgarde SSL after STARTTLS)

## Contributors
* [Vangos Pterneas](http://pterneas.com) from [LightBuzz](http://lightbuzz.com)
* [Alex Borghi](https://it.linkedin.com/pub/alessandro-borghi/75/957/493)
* [Jochen Kalmbach](http://blog.kalmbach-software.de/)
* [PrimalZed](https://github.com/PrimalZed)
* [Radu Mihai Enghis](http://www.enghis.de)
* Based on code by [Sebastien Pertus](http://bit.ly/1q4focT) from [Microsoft](http://microsoft.com)

## Blog post
Read the detailed post by Sebastien [here](http://bit.ly/1q4focT).

## License
You are free to use these libraries in personal and commercial projects by attributing the original creator of the project. [View full License](https://github.com/LightBuzz/SMTP-WinRT/blob/master/LICENSE).
