# HumbleBundleBot

![](http://shields.hust.cc/BUILT%20ON-Azure%20Functions-blue.svg)

Scrapes HumbleBundle.com for new Bundles and posts new bundles to different Discord channels whenever a new Bundle shows up.
Each Bundle can be independently scanned and sent to a different channel, allowing easy organization of messages.

Scraping is done once and results are sent to all relevant webhooks using Serverless Functions and Azure Queue, making the system extremely cheap to run (currently free) and highly scalable.

![](https://puu.sh/y6xli/fbb05197e7.png)

![](https://puu.sh/y6wJs/0426467d2a.png)

## How To Register a Webhook

Make a HTTP POST request to https://humblebundlenotifications.azurewebsites.net/api/RegisterWebhook with a Body of the following format:

```
{
    "type" : <Valid Type Code>,
    "webhook" : "<YOUR DISCORD WEBHOOK URL>",
    "sendUpdates" : <true|false>
}
```

The following types are valid:

| Type        | Type Code | Implemented? |
| ----------- | --------- | ------------ |
| Games       | 0         | YES          |
| Books       | 1         | YES          |
| Mobile      | 2         | YES          |
| Software    | 3         | NO           |
| Monthly     | 4         | NO           |

Example of subscribing to the Games Bundle:

```
{
    "type" : 0,
    "webhook" : "https://discordapp.com/api/webhooks/abcd123...",
    "sendUpdates" : true
}
```

**NOTE: `sendUpdates` and Unimplemented Type's can be registered now, but will not function until support is added. No changes should be needed to be made to start recieving messages once these are supported.**

## Roadmap
- [X] Discord Embed Formatting
- [X] Discord Webhook for message creation
- [X] Azure Table Storage for persistent storage
- [X] Swap from IronWebScraping to ScrapySharp as scraping library
- [X] General Games scraping
- [X] Books Scraping
- [X] Mobile Scraping
- [X] API for registering new webhooks to different bundles rather than current static implementation
- [X] Ability to "unsubscribe" by deleting Webhook registration from system
- [X] Rework entire system as Serverless App
- [X] Polish Embed Formatting
- [X] Software Scraping
- [ ] Micro-site to make Webhook registration / deregistration easier
- [ ] Ability to send out messages about updates to bundles
