# SMTP client for WinRT

An SMTP client for WinRT. Send emails from within your Windows 8 and Windows Phone app.

## Installation
You can download and build this project or simply install it via NuGet:
	
	PM> Install-Package lightbuzz-smtp

## Examples
Import the assembly to your project and include its namespace:
	
	using LightBuzz.SMTP;

### Send an email message
*This is an example of using your own SMTP server. Check below for using Gmail and Outlook.*
  
	EmailClient client = new EmailClient
	{
      		Server = "example.com",
		Port = 25,
	        Username = "info@example.com",
	        Password = "Pa$$w0rd",
	        From = "you@example.com",
	        To = "someone@anotherdomain.com",
	        SSL = false,
	        Subject = "Subject line of your message",
	        Message = "This is an email sent from a WinRT app!"
  	};

  	await client.SendAsync();
  
### Credentials for Gmail
  Server: smtp.gmail.com
  Port: 465
  SSL: True
  
### Credentials for Outlook
  Server: smtp-mail.outlook.com
  Port: 587
  SSL: False (upgarde SSL after STARTTLS)

## Contributors
* [Vangos Pterneas](http://pterneas.com) from [LightBuzz](http://lightbuzz.com)
* [Sebastien Pertus](http://bit.ly/1q4focT) from [Microsoft](http://microsoft.com)

## Blog post
Read the detailed post by Sebastien [here](http://bit.ly/1q4focT).

## License
You are free to use these libraries in personal and commercial projects by attributing the original creator of the project. [View full License](https://github.com/LightBuzz/SMTP-WinRT/blob/master/LICENSE).
