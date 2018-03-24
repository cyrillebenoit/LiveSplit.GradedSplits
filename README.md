# LiveSplit Graded Splits
## Splits List Replacement

A LiveSplit Component that displays a list of split times and deltas in relation to a comparison. 
Automatically replaces the segment icon based on that segment's performance, compared to that segment's best. 
Requires LiveSplit 1.7.5.

General Settings:
* Don't override - Turns this feature on/off
* Override current live run - Updates your split icons as you run (when you split, the icon is updated). Your current split icons remain until you split that segment.
* Override current live run AND personal best - Updates the split icons for your personal best against that segment's best time. Also updates icons when you split.
* Override personal best - Does the same as Override current live run AND personal best (Known issue)

Split Settings:
* Icon Image - Uses a local image file when overriding your normal split icon
* Behind Best Seg By % - Use this icon if your segment time is less than the percentage slower your best segment. For example, if your best segment is 100s, and you specify 10%, the icon shows if you split at 110s or faster. 
* Don't Use - Disables this split icon, it will never override the normal split icon
* Show on (Best/Ahead(Gaining)/etc.) - When you split, if the split is your Best Segment, Ahead (Gaining), etc., use this icon instead of the standard split icon. (Known issue - only overrides for the current live run)

Recommended Settings:
![alt text](https://i.imgur.com/qiJaYDp.png)

