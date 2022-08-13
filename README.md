# SS-Angle-Fixer
.NET 6 program for restoring SS angles that have been run through ss_sector_range.exe

Main usage is for Redump contributers that would like to submit stealth files to abgx360. Since redump likes matching security sector hash, a hidden "feature" in ss_sector_range.exe is that it'll set the angle readings to their default target values (1, 91, 181 and 271). Therefore these SS files cannot be submitted to abgx360, but if you have the log from Xbox Backup Creator, the original angles can be restored.

Simply load your SS.bin file in the left panel, then load the Xbox Backup Creator log on the right panel. If the angles don't match, click "Import", then save your changes.
