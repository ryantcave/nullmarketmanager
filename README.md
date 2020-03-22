# Null Market Manager

### What is this?
Null Market Manager is a C# console application that consumes the EVE Online ESI. The application scrapes the EVE API for all market orders belonging to the user-specified stations for the purpose of finding profitable items for export from an origin location to a destination location and immediately resell.
This is a personal project and designed for my own usecases.

### How does it work?
- All market orders for an origin and destination location are pulled from the EVE api and are processed to find the "best" market orders for each unique item. In this case the best orders are those with the highest price for buy orders and the lowest price for sell orders.
- With this mapping of the best orders, we compare Buy & Sell orders to find a user-configurable profit margin.
- With a list of potential items to purchase, we compare them to all existing orders of their type. If those orders have a price-point that is within a user configurable range (e.g. 2%), the quantity of those items is added to our market order of interest. This is done to give a more accurate picture of supply/demand.
- Orders with an accurate supply/demand are then processed one last time to determine the final profit including the cost to ship the items to the destination. In this case, isk/m^3.
- Results that match a user minimum total profit are sorted and displayed to the user.

### How do I use it?
If you are not a developer, you can't. The application is not currently designed for the endusers, as opening it up in this way would cut into my own profits!
If you are a developer, you will need to register the application in the EVE Developers portal and allow it access to all user scopes. Once this is done, update your Client & Secret ID in the code, and update the Location IDs in the constants class to match your desired origin and destinations.

### Required Setup 

Create `App.config` within the NullMarketManager project directory with the below contents and your API keys filled in:

```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="ClientID" value="{ClientID Goes Here}" />
    <add key="SecretKey" value="{SecretKey Goes Here}" />
  </appSettings>
</configuration>
```

### Will the market manager ever have <idea>?
In a perfect world the manager would have a UI and other market features unrelated to export routes (e.g. notifications when buy orders are outbid). However, as it's not to my advantage to distribute the application, it remains in a state requiring it to be run in Visual Studio.

If I ever stop using the application I may put the finishing touches on it for other players to benefit from it.

### Is this still in development?
If there are active commits from the last 30 days, most likely yes. Otherwise no.
