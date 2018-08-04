# HumbleBundleBot

![](https://img.shields.io/badge/Built%20On-Azure%20Functions-blue.svg)

Scrapes HumbleBundle.com for new Bundles and posts new bundles to different webhooks whenever a new Bundle shows up.
Each Bundle can be sent to a different webhook, allowing easy organization of messages - in Discord, each channel can have webhooks associated with them for integrations like this.

Scraping is done once and results are sent to all relevant webhooks using Serverless Functions and Azure Queue, making the system extremely cheap to run (currently free) and highly scalable.

![](https://s33.postimg.cc/3w0ux45wv/Capture.png)

![](https://s33.postimg.cc/z4e17ukxb/Capture.png)

## Changelog

`5/23/2018` - We now support the ability to register any webhook instead of just Discord webhooks.
If you register a Discord webhook the experience is still the same and you will receive Discord formatted messages.
If you have already registered a webhook before this date, your webhook registration has been defaulted to Discord.

If you register a webhook of type `RawJson` (1), you will receive a `BundleQueue` payload that looks similiar to the following:

```
{
  "Bundle": {
    "Name": "Humble Indie Bundle 19",
    "URL": "https://www.humblebundle.com/games/humble-indie-bundle-19",
    "Description": "SUPERHOT, JYDGE, SOMA, and more – cross-platform, DRM-free, and on Steam!",
    "ImageUrl": "https://humblebundle.imgix.net/misc/files/hashed/632a3bceaaede04cf3d0dee88cfaa7f473cc3fb8.png?auto=compress&h=630&w=1200&s=98e4c242fff7139eceb2bee5740b605b",
    "Type": 0,
    "Sections": [
      {
        "Title": "Pay what you want!",
        "Items": [
          {
            "Name": "Halcyon 6: Lightspeed Edition"
          }
        ]
      },
      {
        "Title": "Beat the Average!",
        "Items": [
          {
            "Name": "Keep Talking and Nobody Explodes"
          }
        ]
      },
      {
        "Title": "Pay $14 or more to also unlock!",
        "Items": [
          {
            "Name": "SUPERHOT"
          },
          {
            "Name": "$2 Humble Wallet credit for Monthly subscribers"
          }
        ]
      }
    ],
    "Items": [
      {
        "Name": "Halcyon 6: Lightspeed Edition"
      }
      {
        "Name": "Keep Talking and Nobody Explodes"
      },
      {
        "Name": "SUPERHOT"
      },
      {
        "Name": "$2 Humble Wallet credit for Monthly subscribers"
      }
    ]
  },
  "IsUpdate": false
}
```

## How To Register a Webhook

*All requests to the endpoints require a header of `Content-Type: application/json`*

Make a HTTP POST request to https://humblebundlenotifications.azurewebsites.net/api/RegisterWebhook with a Body of the following format:

```
{
    "type": <Valid Bundle Type Code>,
    "webhook": "<YOUR DISCORD WEBHOOK URL>",
    "sendUpdates": <true|false>,
    "webhookType": <Valid Webhook Type Code>
}
```

The following Bundle types are valid:

| Type        | Type Code |
| ----------- | --------- |
| Games       | 0         |
| Books       | 1         |
| Mobile      | 2         |
| Software    | 3         |
| Mixed       | 4         |


The following Webhook types are valid:

| Type        | Type Code |
| ----------- | --------- |
| Discord     | 0         |
| RawJson     | 1         |

Example of subscribing to the Games Bundle with a `Discord` webhook:

```
{
    "type": 0,
    "webhook": "https://discordapp.com/api/webhooks/abcd123...",
    "sendUpdates": true,
    "webhookType": 0
}
```

Webhook URLs are encrypted before they are stored.

## How to remove a registered Webhook

*All requests to the endpoints require a header of `Content-Type: application/json`*

Make a HTTP DELETE request to https://humblebundlenotifications.azurewebsites.net/api/DeleteWebhook with a Body of the following format:

```
{
    "type": <Valid Bundle Type Code>,
    "webhook": "<YOUR SUBSCRIBED WEBHOOK URL>",
    "webhookType": 0
}
```

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
- [X] Add data-at-rest encryption to Webhook URL's
- [X] Ability to send out messages about updates to bundles
- [X] Extralife "Special" scraping
- [X] Raw JSON registering
- [ ] Micro-site to make Webhook registration / deregistration easier

