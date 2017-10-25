# HumbleBundleBot

Scrapes HumbleBundle.com for new Bundles and posts new bundles to different Discord channels whenever a new Bundle shows up.

Each Bundle can be independently scanned and sent to a different channel, allowing easy organization of messages.

Scraping is done once and results are sent to all relevant webhooks, making the system extremely cheap to run.

![](https://puu.sh/y6wI0/4087a005b8.png)

![](https://puu.sh/y6wJs/0426467d2a.png)

## Roadmap
- [X] Discord Embed Formatting
- [X] Discord Webhook for message creation
- [X] Azure Table Storage for persistent storage
- [X] Swap from IronWebScraping to ScrapySharp as scraping library
- [X] Serverless function to handle CRON based scheduling
- [X] General Games scraping
- [X] Books Scraping
- [ ] Mobile Scraping
- [ ] Software Scraping
- [ ] Monthly Scraping
- [ ] API for registering new webhooks to different bundles rather than current static implementation
