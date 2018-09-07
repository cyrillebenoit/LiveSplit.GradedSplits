# LiveSplit Graded Splits
## Splits List Replacement

A LiveSplit Component that displays a list of split times and deltas in relation to a comparison. 
Automatically replaces the segment icon based on that segment's performance, compared to that segment's best. 
Requires LiveSplit 1.7.5.

General Settings:
* Don't override - Turns this feature on/off
* Override current live run - Updates your split icons as you run (when you split, the icon is updated). Your current split icons remain until you split that segment.
* Override personal best - Updates your split icons based on how well your personal best did against that segment's best time. Note that as you run more, and get better splits, your Personal Bests' become worse in comparision.
* Override current live run AND personal best - Updates the split icons for your PB AND updates icons when you split.
* Known issue - Using Best Seg/On Ahead (Gaining), etc. with "Override personal best" causes the PB to not be updated based on those criteria. For now, it's best to just stick to a % comparison if you want to have icons for your PB


Split Settings:
* Icon Image - Uses a local image file when overriding your normal split icon
* Behind Best Seg By % - Use this icon if your segment time is less than the percentage slower your best segment. For example, if your best segment is 100s, and you specify 10%, the icon shows if you split at 110s or faster. 
* Don't Use - Disables this split icon, it will never override the normal split icon
* Show on (Best/Ahead(Gaining)/etc.) - When you split, if the split is your Best Segment, Ahead (Gaining), etc., use this icon instead of the standard split icon. (Known issue - only overrides for the current live run)

Recommended Settings:
![alt text](https://i.imgur.com/qiJaYDp.png)

## Installation/Setup

1. Unzip
2. Copy LiveSplit.GradedSplits.dll into the Components folder in the LiveSplit folder. 
3. Run LiveSplit.exe
4. Right-Click -> Edit Layout.
5. Click the + -> List -> Graded Splits

## Importing Settings from your Splits

1. Make a copy of your layout.
2. In your layout, add Graded Splits. KEEP the original Split component in the layout. 
3. Save the layout and exit the LiveSplit.
4. With a text editor, open your layout file (MyLayoutFileName.lsl)
5. Find this section, then copy everything in that section, starting from:
```
    <Component>
      <Path>LiveSplit.Splits.dll</Path>
      <Settings>
           <Version>1.6</Version>
           
           COPY FROM HERE
           <CurrentSplitTopColor>FFB601A5</CurrentSplitTopColor>
           
           ... Lots of stuff...
           
            </Columns>
            TO HERE
            
      </Settings>
    </Component>
```

6. Find this section, then delete everything in the section starting from:
```
    <Component>
      <Path>LiveSplit.GradedSplits.dll</Path>
      <Settings>
        <Version>1.6</Version>
        
        DELETE FROM HERE
        <CurrentSplitTopColor>FFB601A5</CurrentSplitTopColor>
          ... Lots of stuff ...
          
        </Columns>
        TO HERE
        
        <GradedIcons>
             ... Lots off stuff ...
        </GradedIcons>
      </Settings>
    </Component>
```
7. Paste the content you copied there. 
8. Save your changes. 
9. Open up LiveSplit again. Your layout should have 2 sets of splits. 
10. Remove the original split component from your splits.

## Thanks
In no particular order:

Chlorate - https://chlorate.ca/ - for putting in effort with ideas+testing+bug reporting, as well as making the icons, https://chlorate.ca/split-icons/

protomagicalgirl - http://proto.ml - for putting in lots of effort ideas+testing+bug reporting, and advertising!

