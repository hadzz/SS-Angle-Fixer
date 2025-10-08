# SS-Angle-Fixer
.NET 6 program for restoring Xbox 360 DVD security sectors changed by ss_sector_range.exe. After using this, your SS file's hash will match the original dump.

Main use is for Redump contributers that would like to submit to abgx360. Since Redump likes matching security sector hash, a hidden "feature" in ss_sector_range.exe is that it'll set the angle readings to their default target values (1, 91, 181 and 271). These SS files cannot be submitted to abgx360, but if you have the log from Xbox Backup Creator saved, the original file can be restored.

Simply load your SS.bin file in the left and the program will tell you if its necessary to import from the log. If modifications are detected, load the Xbox Backup Creator log on the right. If the log loads sucessfully, click the "Import" button, then click "Overwrite" or "Save As" button to save your restored SS.bin file.

Only built and tested on Windows 10 x64, but code could easily be ported to Linux if you want to make it command line or use mono/another GUI framwork.

Required to run:
  - Windows 7 or later (x86/x64) 
  - .NET 6.0 Runtime Library - Under ".NET Desktop Runtime" https://dotnet.microsoft.com/en-us/download/dotnet/6.0
  
Built with:
  - Microsoft Visual Studio Community 2022 (64-bit) - Version 17.3.0
  - .NET SDK 6.0.400
