# Citizen

## Rules:
1. Each citizen has their own attributes calculated at tick 0
2. Every tick, they will try to form cit coalition with every other tick within talk-span using the criteria:
   1. The coalition must **benefit** them, meaning coalition attributes has to be greater than their own attribute
   <!-- TODO: Update which attributes they update later -->
   2. They will then update their own attributes at the end of each tick
      * Cits in coalition update differently from cits who aren't in coalition

## Note:
None

# Stakeholder

## Rules:
1. Some stakeholders are pre-defined
   1. They joined the cbo coalition forming
   2. They update their scalepower every tick (which leads to sown-power being updated)
2. First citizen of links in every ticks becomes stakeholder
   1. They will join the pre-defined stakeholders and try to form cbo coalition with the same exact criteria as regular cits

## Note:
1. ! Stakeholders aren't affected by talk-span

# Regulator

## Rules:
1. There are **ONLY** pre-defined regulators
2. Regulators try to form coalition with each other based on the same criteria

## Note:
1. ! Regulators aren't affected by talk-span
<!-- ? Pre-defined regulator start updating their stuff at tick 15, but they're only active at tick >= 21, why? -->
16 to 20: regulators bargain with stakeholders. From 21 to 25 they will bargain among themselves

# Note from original SemPro model
## Cit module:
```
Agents survey their surrounding area, set by variable talk-span, and assess the attitude of all other agents with attitudes similar enough to theirs, set by variable tolerance.
If there are any "similar" agents nearby, agents update their attitude to a weighted average of their attitude, their core ideology, and the average of the nearby agents' attitudes.
The weighting is set by variables opinion-stability and opinion-instability.
```

## patch-cbo:
```
This is the process by which community based organizations can arise.  Patches look around and see how much opposition there is.
If there is a lot of opposition, the patch creates a community organization, which can then try to influence the patches nearby.
It's based on the average density of agents in the current model and the average square of opposition.
```

## influece:
```
This is the influence model. I have set it up various ways to allow for different specifications.
In the first way, attitude is pent up, and then released.
In the second way, the agents convey their attitude every iteration.
In the third way, agents convey their attitude every iteration, but only after reaching a certain threshold.
```



# Questions:
1. ? After each tick, pro dev cits normalizes their power with cbo-power of pro dev cits. Same for anti dev cits.
2. ? if a citizen becomes a stakeholder, would they join utility-info or big-ngo?

# Future plan:
1. Implement a check to stop the model after there is no more change
2. Double check scalepower sum [own-power] of cits

utility-info, big-ngo are for certain stakeholders to influence other citizens around them

