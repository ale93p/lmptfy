# Let Me Ping That For You
### Description
LetMePingThatForYou is and old service that I developed for fun in order to check a server availability and warn the administrator in case it came out to be offline.
### How it Works
The main idea of the service workflow is:
* periodically ping an IP address
* in case of multiple failures send a warning email
The xml configuration file allows to set:
* multiple IP addresses
* email configuration (multiple email destination addresses, smtp server, ...)
* ping settings (interval and threshold)
### Future
If I'll find some time I would like to rewrite the service for Linux system, I'll see what I can do.
