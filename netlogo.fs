;See the end of the code for the log of changes to the model.

;Note: set the world size to 257x357 by right-clicking on the world viewer in the
;interface tab and setting max-pxcor to 128 and max-pycor to 178.  This will vary for
;other GIS files.


extensions [gis]
;Load the GIS extension for NetLogo.

;Breed of turtles: citizens who are the people and projos who are connected to the project patches so links can be created.

breed [cits cit]
breed [projos projo]
breed [stakeholders stakeholder]
breed [regulators regulator]


;Create the lines as agents.

breed [block-lines block-line]

;Breed of links: from cits to nearest projo.

directed-link-breed [clopros clopro]
directed-link-breed [linkcits linkcit]
directed-link-breed [linkstakeholders linkstakeholder]
directed-link-breed [linkregulators linkregulator]

;Breed of links: from cits to projos in their "viewshed".

directed-link-breed [sheds shed]

sheds-own [visible]

;Create citizen turtles' (cit) attributes for environmentalism and ideology, as well as associated attitudes that can change.
;Also, create other cit attribute variables that are going to be needed.
;Note, the world has been set to be a defined cartesian plain, with no toroidal properties.

cits-own [ideo idatt ave-id id2 pref tpreference power prox proximity turcbo salience im pent message minlen ran
  temp-eu minpref ;max-temp-eu
  ;own-pref
  ;own-power
  own-eu cbo-pref cbo-power cbo-eu
  prefrange1 prefrange2 prefrange3 prefrange4 prefrange5 prefrange6 prefrange7 prefrange8 prefrange9 prefrange10
  cboprefrange1 cboprefrange2 cboprefrange3 cboprefrange4 cboprefrange5 cboprefrange6 cboprefrange7 cboprefrange8 cboprefrange9 cboprefrange10]
;stakeholders-own [sown-pref sown-power sown-eu scbo-pref scbo-power scbo-eu stemp-eu
;  scboprefrange1 scboprefrange2 scboprefrange3 scboprefrange4 scboprefrange5]
turtles-own [sturcbo? rturcbo? stakeholder? sown-pref sown-power sown-eu scbo-pref scbo-power scbo-eu stemp-eu
  regulator? rown-pref rown-power rown-eu rcbo-pref rcbo-power rcbo-eu rtemp-eu
  scboprefrange1 scboprefrange2 scboprefrange3 scboprefrange4 scboprefrange5 scboprefrange6 scboprefrange7 scboprefrange8 scboprefrange9 scboprefrange10
  rcboprefrange1 rcboprefrange2 rcboprefrange3 rcboprefrange4 rcboprefrange5 rcboprefrange6 rcboprefrange7 rcboprefrange8 rcboprefrange9 rcboprefrange10
  own-pref own-power out-links in-links]

;Projos own a characteristic that tells how much thier linked cits oppose the project.

projos-own [projoim]

;Clopros and sheds, that is, all links, own im of the cit;

links-own [citim citlink? cbolink? regulatorlink?]
linkcits-own [pref1 power1 eu1 pref2 power2 eu2 intereu cbopref cbopower cboeu cboeu1 cboeu2 cboeu12 diffpref1 diffpref2 id]
linkstakeholders-own [spref1 spower1 seu1 spref2 spower2 seu2 sintereu scbopref scbopower scboeu scboeu1 scboeu2 scboeu12 sid bool1 bool2 bool3]
linkregulators-own [rpref1 rpower1 reu1 rpref2 rpower2 reu2 rintereu rcbopref rcbopower rcboeu rcboeu1 rcboeu2 rcboeu12 rid]

;Create the variables from the GIS files and thos that define whether a patch
;has the project on it, and whether there's an active CBO.

patches-own [blarea bname population density popprob patran run? park sub addr ppower lattitude longitude cbo? proj? comm in-view in-project in-comm cbo-dist cbonumb avecbopref]

;Set global variables for ease of plotting and for use of GIS data.

globals [firstid over? number-angry pref-std quick?
         cen-dataset lapa-dataset capa-dataset comm-dataset view-dataset line-dataset;sf-dataset
         scepower scepref scesalience sceim
         isopower isopref isosalience isoim
         cecpower cecpref cecsalience cecim
         landspower landspref landssalience landsim
         parkspower parkspref parkssalience parksim
         fspower fspref fssalience fsim
         blmpower blmpref blmsalience blmim
         epapower epapref epasalience epaim
         nrdcpower nrdcpref nrdcsalience nrdcim
         cbdpower cbdpref cbdsalience cbdim
         sorpower sorpref sorsalience sorim
         mcxpower mcxpref mcxsalience mcxim
         compower compref comsalience comim
         renpower renpref rensalience renim
         stakeim stakeinf
         meaneu totalcbo
         prefvariance cboprefvariance
         scalepower scalepower2
         utilitypref ngopref ngopref1 ngopref2
         regulators-pro regulators-anti
         citizenpref
		 globpref globpow globutil1 globutil2 globcitlink globequals globless globslink globrlink globstempeu1 globstempeu2 globrtempeu1 globrtempeu2 globtempeu1 globtempeu2 globmin1 globmin2
        ; pref1 power1 eu1 pref2 power2 eu2 intereu cbopref cbopower cboeu cboeu1 cboeu2
         ]

;The gis-world procedure sets up the world, and then the initialize procedure sets up the turtles and project.  That is, gis-world is fixed, initialize is random.
; but we only need the area -5 50 -50 5

to gis-world
 ;; (for this model to work with NetLogo's new plotting features,
  ;; __clear-all-and-reset-ticks should be replaced with clear-all at
  ;; the beginning of your setup procedure and reset-ticks at the end
  ;; of the procedure.)
  __clear-all-and-reset-ticks
 set-patch-size 2
 resize-world -100 100 -150 100
 gis-input
 set-patch-size 4
 resize-world -85 20 -105 15
 display-blocks
 apply-variables
end

;The initialize procedure clears any previous information, sets the color of the patches and the initial conditions.
;The initialize-turtles, and label-up procedures are explained below.

to initialize
    if file-exists? "linkOut.csv"
      [ file-delete "linkOut.csv" ]
    if file-exists? "stakeOutCreate.csv"
      [ file-delete "stakeOutCreate.csv" ]
    if file-exists? "scaleStake.csv"
      [ file-delete "scaleStake.csv" ]
          if file-exists? "stakeEquals.csv"
      [ file-delete "stakeEquals.csv" ]
          if file-exists? "stakeLess.csv"
      [ file-delete "stakeLess.csv" ]
          if file-exists? "scbopower.csv"
      [ file-delete "scbopower.csv" ]
          if file-exists? "regOutCreate.csv"
      [ file-delete "regOutCreate.csv" ]
          if file-exists? "regEquals.csv"
      [ file-delete "regEquals.csv" ]
      if file-exists? "regLess.csv"
      [ file-delete "regLess.csv" ]
 reset-ticks
 clear-all-plots
 clear-output
 set quick? 1
; ask cits [die]
 ask turtles [die]
 ask projos [die]
 ask clopros [die]
; stakeholders
 set stakeinf 0
 ask patches[
  set pcolor 38
  set proj? 0
  set cbo? 0
  set run? 0
  ifelse not (is-number? sub) and not (is-number? addr) [set park 0] [set park 1]
  if not is-number? population [
     set population 0]
  ifelse is-number? blarea
   [if-else blarea > 0
     [set density density]
   [set density 0]]
   [set density 0]
 ]
 project
 comment
 initialize-turtles
 stakeholder-setup
 regulator-setup
 label-up
 set firstid (sum [idatt] of cits)
 set over? 0
 set globcitlink 0
 export-world (word "t0.csv")
end


;-227 to 226 in x-cor, -337 to 337 in the y-cor, for a total of 674  in y, resized to place the project well.

;The gis-input, project, initialize-turtles, and label-up procedures are explained below.

to gis-input
  ;make sure the dataset, in this case cen-dataset, are set as global variables above.
  set-current-directory user-directory
  ;Asks user to set the directory for finding files,
  ;this will be important for multi-computer useability.
  gis:load-coordinate-system ("SoCALbg2010_TRTP_Extent_DATA_NUMBERS.prj")
  ;Selects the projection file for reading the othert GIS files
  ;If the model is to evaluate multiple different areas, a variable could be defined for the project
  ;and then logic could match the various GIS file types for that project
  set line-dataset gis:load-dataset "TRTP_Route_Project.shp"
  set cen-dataset gis:load-dataset "SoCALbg2010_TRTP_Extent_DATA_NUMBERS.shp"
  set view-dataset gis:load-dataset "ViewshedBuffer_polygon.shp"
  ;This brings in the GIS shape data.  NetLogo only accepts .shp files (not directly .shx or .dbf,
  ;the index and database files), but it appears that when you bring in the .shp it automatically
  ;gets the other two because when I didn't have them in the directory it ran an error telling me
  ;that the .dbf file didn't exist.
  ;Now, repeat for the other data sets, including state parks, county parks.
  set capa-dataset gis:load-dataset "Parks_CA2011.shp"
  gis:load-coordinate-system ("Parks_LACo.prj")
  set lapa-dataset gis:load-dataset "Parks_LACo.shp"
  set comm-dataset gis:load-dataset "Comments_NUMBERS.shp"
  gis:set-world-envelope (gis:envelope-of cen-dataset)
  ;gis:set-world-envelope (gis:envelope-union-of gis:envelope-of cen-dataset gis:envelope-of line-dataset)
  ;This sets the borders of the world to be those of the dataset.  It's unclear if its necessary
  ;with only one dataset, but if there are multiple datasets use
  ; gis:set-world-envelope (gis:envelope-union-of (gis:envelope-of ... and repeats the gis:envelope-of
  ;part for each dataset and then close the two parentheses.  In this case, I don't want to include the
  ;others as one is the whole state.
end

;Income is estimated by assigning mid-point values to the census bins.
;Education is also estimated by assigning mid-point values to the census bins.
;Power is calculated as the square root of the product of the relative education and relative income.
;By relative, I mean each is normalized by the average value, so the average value is 1.
;The overall power is then renormalized (because income and education are not uncorrelated) so the average power is 1.

;This procedures draws on the boundaries of the blocks.  It sets the drawing color
;and then tells NetLogo to use the shapes in the GIS dataset that was defined in the
;gis-input procedure above. I use white for census, green for state parks and
;yellow for county parks.

to display-blocks
  ask block-lines [ die ]
  gis:set-drawing-color white
  gis:draw cen-dataset 1
  gis:set-drawing-color green
  gis:draw capa-dataset 1
  gis:set-drawing-color yellow
  gis:draw lapa-dataset 5
  ;gis:set-drawing-color green
  ;gis:draw view-dataset 1
  gis:set-drawing-color red
  gis:draw line-dataset 2
  gis:set-drawing-color 135
  gis:draw comm-dataset 1
end

;This procedure applies data in the GIS dataset to the patches.  The variables must be
;given to the patches in the patches-own statement up at the beginning, and the name
;in parentheses must match the name in the GIS dataset.

to apply-variables
  gis:apply-coverage cen-dataset "ALAND10_NO" blarea
  gis:apply-coverage cen-dataset "GEOID10_NO" bname
  gis:apply-coverage cen-dataset "POP_TOTAL_" population
  gis:apply-coverage cen-dataset "INTPLAT10_" lattitude
  gis:apply-coverage cen-dataset "INTPLTON10" longitude
  gis:apply-coverage cen-dataset "POWER_NO" ppower
  gis:apply-coverage cen-dataset "DENSITY_SQ" density
  gis:apply-coverage capa-dataset "SUBTYPE" sub
  gis:apply-coverage lapa-dataset "ADDR1" addr
  gis:apply-coverage comm-dataset "CONCERNCOD" comm
  ;gis:apply-coverage view-dataset "VIEW" in-view
  ;gis:apply-coverage line-dataset "PROJECT2" in-project
end


;The project procedure defines which patches constitute the trainsmission line.  It can be vertical, horizontal, or diagonal.
;Project patches are set to black.

to project
  gis:set-drawing-color black
  gis:draw line-dataset 1
;All the shapes in the view-dataset have in-view = 1 and none outside do, so this will define a patchset for the project and then mark them as proj?=1
  ask patches [
   if gis:intersects? line-dataset self [
    set proj? 1]]
  ask patches [
   if gis:intersects? comm-dataset self [
    set pcolor 135 set in-comm 1]]
  ask patches [
    if in-project = 1 [ sprout 1
      [ set breed projos
        set color black
        set shape "circle" ]]]
  ask patches [
    if in-comm = 1 [
      sprout 1
      [ set breed cits
        set color 135
        set shape "circle" ]
     sprout 1
      [ set breed cits
        set pref random 70 + 30
        set own-pref random 70 + 30
        set power  [ppower * 2.1] of patch-here
        set own-power  [ppower * 2.1] of patch-here
        set shape "person" ] ]]
end

to comment
  ask patches [
    if comm = 1 [ set pcolor 135]]
  ask patches [
    if comm = 2 [ set pcolor 125]]
  ask patches [
    if comm = 3 [ set pcolor 115]]
  ask patches [
    if comm = 4 [ set pcolor 105]]
  ask patches [
    if comm = 5 [ set pcolor 95]]
  ask patches [
    if comm = 6 [ set pcolor 85]]
end

;Now, create the agents, randomly placed around the work, as people. Note that the variable initial-number is set in the interface with a slider.
;Agents initial ideology, representing a composite of their ideological predisposition against the project, is set randomly.
;Agents' initial attitude is equal to that ideology plus a random term.
;Variable madcount is 1+ the number of times they have been "turned off" by the NGO and is a measure of how much they will ignore the NGO.
;Variable prox is how close a project patch is.  Specifically, the minimum distance to a black patch.
;Variable proximity is the inverse of prox and is a measure of how much the project will affect the agent, and thus how much he/she cares about it.
;Variable annoy is a random variable to represent a random distribution of people's annoyance with a project.
;Variable pent is used in influence model 1, in which opinion about the project is only communicated once enough is "pent up".
;Variable message is used in influence models 2 and 3 (we use 2) and represents how strong of a sentiment, and in which direction, agents communicate.
;Creates a projo on each project patch, but makes it black (i.e. invisible)

to initialize-turtles
  let areapop (sum [density] of patches)
  show areapop
  ask patches [
   set popprob ( (initial-number * density) / (areapop + 1))
   set patran random-float 1]
  ask patches [
    sprout-cits 1
    if popprob > patran [
    set run? 1] ]
  ask stakeholders [die]
  ask cits [ setxy random-xcor random-ycor
    if [run?] of patch-here = 0
     [die]
    set shape "person"
    set ideo random-normal  60 10
    set ran random 0 + 100
    set idatt (ideo * 0.9 + ran * 0.1)
    ifelse powdif?
      [set power  [ppower * 2] of patch-here]
      [set power 1]
    set prox  ceiling distance min-one-of patches with [proj? = 1] [distance myself]
    set proximity ( 1 / (prox) )
    set pent 0
    set message 0
    set pref (((disruption * proximity * 100)  +  idatt )/ 2)
    ifelse turcbo != 2 [
    set own-power [ppower * 2] of patch-here
    set own-pref (((proximity * 100)  +  idatt )/ 2)
    set own-eu (100 - abs (own-pref - own-pref)) * own-power]
      [set own-power [ppower * 2] of patch-here
        set own-pref cbo-pref
        set own-eu (100 - abs (cbo-pref - cbo-pref)) * cbo-power]
    set turcbo 1
  ]
  ask patches [ if proj? = 1
    [sprout-projos 1 [set color white set shape "circle"]]]
  ;nearest
  view-shed
end

;This procedure asks each cit to find the nearest project segment.  This allows us to associate the influence message of each turtle to a project patch
;and monitor where the project is controversial and where it is not.

to nearest
   ask cits [
     let nearest-projo min-one-of projos [distance myself]
     create-clopro-to nearest-projo]
end

;This procedure asks cits to find all project segments in their vicinity, defined by the variable shed-length.  They then create a shed link to all of those projos.
;the green lines can be made visible or invisible with the view-shed-green buttom

to view-shed
  ask cits [
    let shed-pros projos in-radius shed-length]
end

to view-shed-green
  ask cits [
    let shed-pros projos in-radius shed-length
    create-sheds-to shed-pros [set color green set visible 2]]
ask sheds with [ visible > 0 ]
[ set visible visible - 1
if visible = 0
[ die ]]
end

;The go procedure runs the iterations of the model.  The procedures are described below.

to go
  if over? = 1 [stop]
  reset-minpref
  turtle-talk
  ;print-stakes
  stakeholder-setup
  regulator-setup
  stakeholder-talk
  if regulate? [regulator-talk]
 ; turtle-match
 ; report-meaneu
 ; report-totalcbo
  utility-info
  big-ngo
  merge-cbo
  regulator-vote
  patch-cbo
  influence
  label-up
  if ticks = 25 [file-close-all]
  report-pref
  report-cbopref
  do-output
  create-plots
  continue
  ifelse regulate? [if ticks = 25 [stop]][if ticks = 20 [stop]]
;  [if ticks = 20 [continue]]
;  ifelse regulate? [if ticks = 25 [success]]
;  [if ticks = 20 [success]]
;  ifelse regulate? [if ticks = 25 [stop]]
;  [if ticks = 20 [stop]]
  tick
end

;Agents survey their surrounding area, set by variable talk-span, and assess the attitude of all other agents with attitudes similar enough to theirs,
;set by variable tolerance.
;If there are any "similar" agents nearby, agents update their attitude to a weighted average of their attitude, their core ideology, and the average of
;the nearby agents' attitudes.  The weighting is set by variables opinion-stability and opinion-instability.

to reset-minpref
  ask cits[
    set temp-eu 0
    set minpref 100000
  ]
    ask turtles with [stakeholder? = 1] [
      set stemp-eu 0
   ]
    ask turtles with [regulator? = 1] [
      set rtemp-eu 0
    ]
end

to turtle-talk
  if ticks = 0 [
  file-open "linkOut.csv"
file-print (word "stages,link number,xcor,xcor,ycor,temp-eu,own-eu,xcor,xcor,temp-eu,own-eu")
file-close-all
  file-open "linkOut.csv"
file-print (word "tick 0 create")
file-close-all
  ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 1
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
     set globmin1 diffpref1
     set globmin2 diffpref2

     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 1]
       [set temp-eu 0
        ;set minpref 100000
         ]
       [
       if globtempeu1 > temp-eu [set temp-eu globtempeu1]
         if minpref > globmin1 [set minpref globmin1]]]

     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 1]
       [set temp-eu 0
        ;set minpref 100000
         ]
       [ ; Does the same as above
         if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]

     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 1]]
     set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
               file-close-all
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
file-close-all
     set globcitlink 1 + globcitlink
    ]]

    file-open "linkOut.csv"
file-print (word "eval equals")
file-close-all

  ask linkcits with [citlink? = 1] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
           file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
file-close-all
      die
    ]
  ]

  ]





  if ticks = 1 [
    file-open "linkOut.csv"
file-print (word "tick 1 create")
file-close-all
  ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 2
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
     set globtempeu1 cboeu1
     set globtempeu2 cboeu2
     set globmin1 diffpref1
     set globmin2 diffpref2

     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 2]
       [set temp-eu 0]
       [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]

     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 2]
       [set temp-eu 0]
       [ ; Does the same as above
         if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 2]]
     
     set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     set globcitlink 1 + globcitlink
    ]]

  file-open "linkOut.csv"
file-print (word "eval equals")
file-close-all

  ask linkcits with [citlink? = 2] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
                 file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
      set hidden? FALSE
      ]

    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                       file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
      die
  ]


     ask cits with [stakeholder? = 1] [
     ;user-message (word "This is " xcor)
     ;user-message (word "There are " count my-out-links with [citlink? > 0] " outlinks.")
     ;user-message (word "There are " count my-in-links with [citlink? > 0] " inlinks.")
     ]
  ]
  


  file-open "linkOut.csv"
file-print (word "eval less")
file-close-all

   ask linkcits with [citlink? < 2] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [

     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
   file-close-all
   
    ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))

    [ask end1 [
        if count my-out-links with [citlink? = 2] = 0 and count my-in-links with [citlink? = 2] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 2] = 0 and count my-in-links with [citlink? = 2] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
                     file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      die]


    [ask end1 [
        ifelse count my-out-links with [citlink? = 2] = 0 and count my-in-links with [citlink? = 2] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 2] = 0 and count my-in-links with [citlink? = 2] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
    file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
  file-close-all
    ]

    ]
    
    ]
   ask cits with [stakeholder? = 1] [
     ;user-message (word "This is " xcor)
     ;user-message (word "There are " count my-out-links with [citlink? > 0] " outlinks.")
     ;user-message (word "There are " count my-in-links with [citlink? > 0] " inlinks.")
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
     file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
    file-close-all
     ]
  ]









  if ticks = 2 [
      file-open "linkOut.csv"
file-print (word "tick 2 create")
 file-close-all
  ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 3
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
          set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 3]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 3]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 3]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
  file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 3] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                 file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                       file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 3] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
       set pref1 [own-pref] of end1
       set power1 [own-power] of end1
       set eu1 [own-eu] of end1
       set pref2 [own-pref] of end2
       set power2 [own-power] of end2
       set eu2 [own-eu] of end2
       set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
       set cboeu1 0.5 * (1.5 * eu1 + intereu)
       set cboeu2 0.5 * (1.5 * eu2 + intereu)
       set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
       set cbopower (power1 + power2) * 1.5
       set cboeu cbopower * (100 - abs (cbopref - cbopref))
       set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
            file-open "linkOut.csv"
  file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
      file-close-all

      ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
      [ask end1 [
          if count my-out-links with [citlink? = 3] = 0 and count my-in-links with [citlink? = 3] = 0 [
          set turcbo 1
          set cbo-pref 0
          set cbo-power 0
          set stakeholder? 0
          set own-power own-power / 1.5
          set own-eu (100 - abs (own-pref - own-pref)) * own-power
          set shape "person"]]
      ask end2 [
          if count my-out-links with [citlink? = 3] = 0 and count my-in-links with [citlink? = 3] = 0 [
          set turcbo 1
          set cbo-pref 0
          set cbo-power 0
          set stakeholder? 0
          set own-power own-power / 1.5
          set own-eu (100 - abs (own-pref - own-pref)) * own-power
          set shape "person"]]
        file-open "linkOut.csv"
  file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
       file-close-all
        die]
      [ask end1 [
          ifelse count my-out-links with [citlink? = 3] = 0 and count my-in-links with [citlink? = 3] = 0 [
          set turcbo 2
          set stakeholder? 0
          set own-pref [cbo-pref] of other-end
          set cbo-pref [cbo-pref] of other-end
          set cbo-power 0
          set own-eu (100 - abs (own-pref - own-pref)) * own-power]
          [set stakeholder? 1]]
      ask end2 [
          ifelse count my-out-links with [citlink? = 3] = 0 and count my-in-links with [citlink? = 3] = 0 [
          set turcbo 2
          set stakeholder? 0
          set own-pref [cbo-pref] of other-end
          set cbo-pref [cbo-pref] of other-end
          set cbo-power 0
          set own-eu (100 - abs (own-pref - own-pref)) * own-power]
          [set stakeholder? 1]]
      set hidden? FALSE
          file-open "linkOut.csv"
  file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    ]
    ]
   ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
          file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

  if ticks = 3 [
      file-open "linkOut.csv"
file-print (word "tick 3 create")
  file-close-all
  ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 4
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 4]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 4]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 4]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
     file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
  file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 4] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                 file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                       file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 4] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 4] = 0 and count my-in-links with [citlink? = 4] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 4] = 0 and count my-in-links with [citlink? = 4] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
                     file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 4] = 0 and count my-in-links with [citlink? = 4] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 4] = 0 and count my-in-links with [citlink? = 4] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
        file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]
    ]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
          file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

  if ticks = 4 [
      file-open "linkOut.csv"
file-print (word "tick 4 create")
  file-close-all
  ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 5
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 5]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 5]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 5]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
  file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 5] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                 file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                       file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 5] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 5] = 0 and count my-in-links with [citlink? = 5] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 5] = 0 and count my-in-links with [citlink? = 5] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
                     file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 5] = 0 and count my-in-links with [citlink? = 5] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 5] = 0 and count my-in-links with [citlink? = 5] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
        file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]
    ]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
          file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 5 [
        file-open "linkOut.csv"
file-print (word "tick 5 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 6
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 6]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 6]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 6]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 6] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                 file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                       file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 6] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 6] = 0 and count my-in-links with [citlink? = 6] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 6] = 0 and count my-in-links with [citlink? = 6] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
      file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 6] = 0 and count my-in-links with [citlink? = 6] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 6] = 0 and count my-in-links with [citlink? = 6] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
        file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]
    ]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 6 [
        file-open "linkOut.csv"
file-print (word "tick 6 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 7
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 7]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 7]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 7]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  ask linkcits with [citlink? = 7] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 7] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 7] = 0 and count my-in-links with [citlink? = 7] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 7] = 0 and count my-in-links with [citlink? = 7] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 7] = 0 and count my-in-links with [citlink? = 7] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 7] = 0 and count my-in-links with [citlink? = 7] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 7 [
        file-open "linkOut.csv"
file-print (word "tick 7 create")
file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 8
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 8]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 8]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 8]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 8] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 8] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 8] = 0 and count my-in-links with [citlink? = 8] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 8] = 0 and count my-in-links with [citlink? = 8] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 8] = 0 and count my-in-links with [citlink? = 8] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 8] = 0 and count my-in-links with [citlink? = 8] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 8 [
        file-open "linkOut.csv"
file-print (word "tick 8 create")
file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 9
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 9]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 9]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 9]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 9] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 9] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 9] = 0 and count my-in-links with [citlink? = 9] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 9] = 0 and count my-in-links with [citlink? = 9] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 9] = 0 and count my-in-links with [citlink? = 9] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 9] = 0 and count my-in-links with [citlink? = 9] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 9 [
        file-open "linkOut.csv"
file-print (word "tick 9 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 10
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 10]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 10]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 10]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 10] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 10] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 10] = 0 and count my-in-links with [citlink? = 10] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 10] = 0 and count my-in-links with [citlink? = 10] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 10] = 0 and count my-in-links with [citlink? = 10] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 10] = 0 and count my-in-links with [citlink? = 10] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 10 [
        file-open "linkOut.csv"
file-print (word "tick 10 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 11
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 11]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 11]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 11]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 11] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 11] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 11] = 0 and count my-in-links with [citlink? = 11] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 11] = 0 and count my-in-links with [citlink? = 11] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 11] = 0 and count my-in-links with [citlink? = 11] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 11] = 0 and count my-in-links with [citlink? = 11] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 11 [
        file-open "linkOut.csv"
file-print (word "tick 11 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 12
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 12]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 12]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 12]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 12] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 12] [
          file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 12] = 0 and count my-in-links with [citlink? = 12] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 12] = 0 and count my-in-links with [citlink? = 12] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 12] = 0 and count my-in-links with [citlink? = 12] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 12] = 0 and count my-in-links with [citlink? = 12] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 12 [
        file-open "linkOut.csv"
file-print (word "tick 12 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 13
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 13]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 13]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 13]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 13] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 13] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 13] = 0 and count my-in-links with [citlink? = 13] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 13] = 0 and count my-in-links with [citlink? = 13] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 13] = 0 and count my-in-links with [citlink? = 13] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 13] = 0 and count my-in-links with [citlink? = 13] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 13 [
        file-open "linkOut.csv"
file-print (word "tick 13 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 14
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 14]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 14]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 14]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 14] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 14] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 14] = 0 and count my-in-links with [citlink? = 14] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 14] = 0 and count my-in-links with [citlink? = 14] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 14] = 0 and count my-in-links with [citlink? = 14] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 14] = 0 and count my-in-links with [citlink? = 14] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 14 [
        file-open "linkOut.csv"
file-print (word "tick 14 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 15
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 15]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 15]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 15]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 15] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 15] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 15] = 0 and count my-in-links with [citlink? = 15] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 15] = 0 and count my-in-links with [citlink? = 15] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 15] = 0 and count my-in-links with [citlink? = 15] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 15] = 0 and count my-in-links with [citlink? = 15] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 15 [
        file-open "linkOut.csv"
file-print (word "tick 15 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 16
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 16]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 16]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 16]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 16] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 16] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 16] = 0 and count my-in-links with [citlink? = 16] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 16] = 0 and count my-in-links with [citlink? = 16] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 16] = 0 and count my-in-links with [citlink? = 16] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 16] = 0 and count my-in-links with [citlink? = 16] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 16 [
        file-open "linkOut.csv"
file-print (word "tick 16 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 17
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 17]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 17]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 17]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 17] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 17] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 17] = 0 and count my-in-links with [citlink? = 17] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 17] = 0 and count my-in-links with [citlink? = 17] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 17] = 0 and count my-in-links with [citlink? = 17] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 17] = 0 and count my-in-links with [citlink? = 17] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 17 [
        file-open "linkOut.csv"
file-print (word "tick 17 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 18
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 18]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 18]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 18]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
         file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 18] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
   [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 18] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 18] = 0 and count my-in-links with [citlink? = 18] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 18] = 0 and count my-in-links with [citlink? = 18] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 18] = 0 and count my-in-links with [citlink? = 18] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 18] = 0 and count my-in-links with [citlink? = 18] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 18 [
        file-open "linkOut.csv"
file-print (word "tick 18 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 19
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 19]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 19]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 19]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 19] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
     [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 19] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 19] = 0 and count my-in-links with [citlink? = 19] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 19] = 0 and count my-in-links with [citlink? = 19] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 19] = 0 and count my-in-links with [citlink? = 19] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 19] = 0 and count my-in-links with [citlink? = 19] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 19 [
        file-open "linkOut.csv"
file-print (word "tick 19 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 20
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 20]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 20]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 20]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 20] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 20] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
            ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 20] = 0 and count my-in-links with [citlink? = 20] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 20] = 0 and count my-in-links with [citlink? = 20] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 20] = 0 and count my-in-links with [citlink? = 20] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 20] = 0 and count my-in-links with [citlink? = 20] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    if ticks = 20 [
        file-open "linkOut.csv"
file-print (word "tick 20 create")
    file-close-all
    ask cits [
    create-linkcits-to cits in-radius talk-span with [who != [who] of myself]
    [set citlink? 21
     set hidden? TRUE
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
     set diffpref1 abs(cbopref - pref1)
     set diffpref2 abs(cbopref - pref2)
          set globtempeu1 cboeu1
     set globtempeu2 cboeu2
               set globmin1 diffpref1
     set globmin2 diffpref2
     ask end1 [ifelse empty? [cboeu1] of my-out-links with [citlink? = 21]
       [set temp-eu 0] [
if globtempeu1 > temp-eu [set temp-eu globtempeu1]
if minpref > globmin1 [set minpref globmin1]]]
     ask end2 [ifelse empty? [cboeu1] of my-in-links with [citlink? = 21]
       [set temp-eu 0]
       [ ; Does the same as above
if globtempeu2 > temp-eu [set temp-eu globtempeu2]
         if minpref > globmin2 [set minpref globmin2]]]
     ;ask end2 [set temp-eu [cboeu1] of my-in-links with [citlink? = 21]]
          set id globcitlink
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
          file-close-all
          file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
     file-close-all
     set globcitlink 1 + globcitlink
    ]]
    file-open "linkOut.csv"
file-print (word "eval equals")
  file-close-all
  ask linkcits with [citlink? = 21] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
   file-close-all
   ; if ([temp-eu] of end1 <= [own-eu] of end1) or ([temp-eu] of end2 <= [own-eu] of end2)
   ; [die]
    if cboeu1 < [temp-eu] of end1 [die]
    if cboeu1 < [own-eu] of end1 [die]
    if cboeu2 <= [own-eu] of end2 [die]
    if diffpref2 > [minpref] of end2 [die]
    ;if ([temp-eu] of end1) + ([temp-eu] of end2) != (cboeu1 + cboeu2)
    ;[die]
    ifelse ( (precision ([temp-eu] of end1) 6) > (precision ([own-eu] of end1) 6) and ( (precision ([temp-eu] of end2) 6) > ( precision ([own-eu] of end2) 6)))
    [
      set globpref cbopref
      set globpow cbopower
      set globutil1 cboeu1
      set globutil2 cboeu2
      ask end1 [
        set turcbo 2
        set stakeholder? 1
        set own-pref globpref
        set sown-pref globpref
        set cbo-pref globpref
        set own-power 1.5 * own-power
        set sown-power globpow
        set cbo-power globpow
        set own-eu globutil1
        set sown-eu globutil1
        set cbo-eu globutil1
      ]
      ask end2 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
       ; set own-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set sown-pref max [cbopref] of my-in-links with [citlink? = 1]
       ; set cbo-pref max [cbopref] of my-in-links with [citlink? = 1]
        set cbo-pref [cbo-pref] of other-end
        set own-power 1.5 * own-power
       ; set own-power 0
       ; set cbo-power max [cbopower] of my-in-links with [citlink? = 1]
        set cbo-power 0
        set own-eu globutil2
       ; set sown-eu max [cboeu2] of my-in-links with [citlink? = 1]
        set cbo-eu globutil2
      ]
      set hidden? FALSE
                       file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      ]
    [ask end1 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
      ask end2 [
        set turcbo 1
        set own-pref own-pref
        set own-power own-power]
                             file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," [temp-eu] of end1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," [temp-eu] of end2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die
  ]
  ]
  file-open "linkOut.csv"
file-print (word "eval less")
   file-close-all
   ask linkcits with [citlink? < 21] [
     file-open "linkOut.csv"
file-print (word "link number," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
     file-close-all
     if [own-pref] of end1 != [own-pref] of end2 [
     set pref1 [own-pref] of end1
     set power1 [own-power] of end1
     set eu1 [own-eu] of end1
     set pref2 [own-pref] of end2
     set power2 [own-power] of end2
     set eu2 [own-eu] of end2
     set intereu (power1 + power2) * 1.5 * (100 - abs(pref1 - pref2))
     set cboeu1 0.5 * (1.5 * eu1 + intereu)
     set cboeu2 0.5 * (1.5 * eu2 + intereu)
     set cbopref ((pref1 * power1 + pref2 * power2)/(power1 + power2 + 0.0000001))
     set cbopower (power1 + power2) * 1.5
     set cboeu cbopower * (100 - abs (cbopref - cbopref))
     set cboeu12 0.5 * (eu1 + intereu) + 0.5 * (eu2 + intereu)
               file-open "linkOut.csv"
file-print (word "extra," pref1 "," power1 "," eu1 "," pref2 "," power2 "," eu2 "," intereu "," cboeu1 "," cboeu2)
    file-close-all
    ifelse ( (precision (cboeu1) 6) < (precision ([own-eu] of end1) 6 )) or ( (precision (cboeu2) 6) < (precision ([own-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [citlink? = 21] = 0 and count my-in-links with [citlink? = 21] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
    ask end2 [
        if count my-out-links with [citlink? = 21] = 0 and count my-in-links with [citlink? = 21] = 0 [
        set turcbo 1
        set cbo-pref 0
        set cbo-power 0
        set stakeholder? 0
        set own-power own-power / 1.5
        set own-eu (100 - abs (own-pref - own-pref)) * own-power
        set shape "person"]]
          file-open "linkOut.csv"
file-print (word "b1," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
      file-close-all
      die]
    [ask end1 [
        ifelse count my-out-links with [citlink? = 21] = 0 and count my-in-links with [citlink? = 21] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    ask end2 [
        ifelse count my-out-links with [citlink? = 21] = 0 and count my-in-links with [citlink? = 21] = 0 [
        set turcbo 2
        set stakeholder? 0
        set own-pref [cbo-pref] of other-end
        set cbo-pref [cbo-pref] of other-end
        set cbo-power 0
        set own-eu (100 - abs (own-pref - own-pref)) * own-power]
        [set stakeholder? 1]]
    set hidden? FALSE
            file-open "linkOut.csv"
file-print (word "b2," id ",End1 xcor," [xcor] of end1 "," [ycor] of end1 "," cboeu1 "," [own-eu] of end1 ",End2 xcor," [xcor] of end2 "," [ycor] of end2 "," cboeu2 "," [own-eu] of end2 ",," [minpref] of end1 "," [own-power] of end1 "," [own-pref] of end1 "," [own-eu] of end1 "," [stakeholder?] of end1 ",," [minpref] of end2 "," [own-power] of end2 "," [own-pref] of end2 "," [own-eu] of end2 "," [stakeholder?] of end2 "," [turcbo] of end1 "," [turcbo] of end2)
    file-close-all
    ]]
    ]
     ask cits with [stakeholder? = 1] [
     set cbo-power (sum [cbopower] of my-out-links with [citlink? > 0]) + (sum [cbopower] of my-in-links with [citlink? > 0]) - own-power * ((count my-out-links with [citlink? > 0]) + (count my-in-links with [citlink? > 0]) - 1)
               file-open "linkOut.csv"
file-print (word "cord," xcor "," ycor ",sumpower," cbo-power "," sum [cbopower] of my-out-links with [citlink? > 0] "," sum [cbopower] of my-in-links with [citlink? > 0] "," own-power "," count my-out-links with [citlink? > 0] "," count my-in-links with [citlink? > 0])
     file-close-all
     ]
  ]

    ask cits [
      if (count my-out-links with [citlink? > 0] = 0) and (count my-in-links with [citlink? > 0] = 0)[
      set turcbo 1
      set cbo-power 0
      set stakeholder? 0
      set shape "person"]
      if (cbo-power = 0) and (cbo-pref = 0) [
        set shape "person"]
      ]
    ask linkcits [
      file-open "linkOut.csv"
file-print (word "stakes," id "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2)
      file-close-all
      if [cbo-power] of end1 != 0 and [cbo-power] of end2 != 0 [
       ask end2 [
         set cbo-power 0
         set stakeholder? 0]]
        ]
end


to stakeholder-setup
  file-open "scaleStake.csv"
  if ticks = 1 [
  set scalepower sum [own-power] of cits
  file-print (word "scale " ticks)
  file-print (word "scalepower," scalepower)
  create-stakeholders 1 [
    set label "CPUC"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -5
    set heading 0
    fd 15
    set color 115
    set sown-pref 28
    set sown-power scalepower * 2.07 / 18.5265
        file-print (word "CPUC," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "Cal EPA"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -6
    set heading 22.5
    fd 15
    set color 115
    set sown-pref 36
    set sown-power scalepower * 0.93 / 18.5265
        file-print (word "EPA," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "Cal Water"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -7
    set heading 45
    fd 15
    set color 115
    set sown-pref 36
    set sown-power scalepower * 1.07 / 18.5265
        file-print (word "Cal Water," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "Cal Fish"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -8
    set heading 67.5
    fd 15
    set color 115
    set sown-pref 36
    set sown-power scalepower * 1.3 / 18.5265
        file-print (word "Cal Fish," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "Cal Park"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -9
    set heading 90
    fd 15
    set color 115
    set sown-pref 33
    set sown-power scalepower * 0.96 / 18.5265
        file-print (word "Cal Park," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "Cal Forest"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -10
    set heading 112.5
    fd 15
    set color 115
    set sown-pref 34
    set sown-power scalepower * 1.26 / 18.5265
        file-print (word "Cal Forest," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "BLM"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -11
    set heading 135
    fd 15
    set color 115
    set sown-pref 36
    set sown-power scalepower * 1.19 / 18.5265
    file-print (word "BLM," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "Fish & Wildlife"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -12
    set heading 157.5
    fd 15
    set color 115
    set sown-pref 34
    set sown-power scalepower * 0.89 / 18.5265
        file-print (word "Fish & Wildlife," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "EPA"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -13
    set heading 180
    fd 15
    set color 115
    set sown-pref 38
    set sown-power scalepower * 1.04 / 18.5265
        file-print (word "EPA," sown-power)
    set stakeholder? 1]
create-stakeholders 1 [
    set label "SCE"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -14
    set heading 202.5
    fd 15
    set color 105
    set sown-pref 13
    set sown-power scalepower * 1.04 / 18.5265
        file-print (word "SCE," sown-power)
    set stakeholder? 1]
create-stakeholders 1 [
    set label "NRDC"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -15
    set heading 225
    fd 15
    set color 125
    set sown-pref 56
    set sown-power scalepower * 1.11 / 18.5265
        file-print (word "NRDC," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "Sierra"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -16
    set heading 247.5
    fd 15
    set color 135
    set sown-pref 66
    set sown-power scalepower * 1.11 / 18.5265
        file-print (word "Sierra," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "CBD"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -17
    set heading 270
    fd 15
    set color 135
    set sown-pref 75
    set sown-power scalepower * 1 / 18.5265
        file-print (word "CBD," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "Cal Vehicle"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -18
    set heading 292.5
    fd 15
    set color 125
    set sown-pref 47
    set sown-power scalepower * 0.67 / 18.5265
        file-print (word "Cal Vehicle," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "City"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -19
    set heading 315
    fd 15
    set color 125
    set sown-pref 54
    set sown-power scalepower * 0.96 / 18.5265
        file-print (word "City," sown-power)
    set stakeholder? 1]
  create-stakeholders 1 [
    set label "Renewable"
    set label-color black
    set shape "circle"
    set size 3
    setxy 0 -20
    set heading 337.5
    fd 15
    set color 115
    set sown-pref 28
    set sown-power scalepower * 0.93 / 18.5265
        file-print (word "Renewable," sown-power)
    set stakeholder? 1]
  ]

  if ticks > 1 [
      file-print (word "scale " ticks)
  file-print (word "scalepower," scalepower)
  set scalepower sum [own-power] of cits
    ask turtles with [label = "CPUC"][
    set sown-power scalepower * 2.07 / 18.5265
        file-print (word "CPUC," sown-power)
    ]
    ask turtles with [label = "Cal EPA"][
    set sown-power scalepower * 0.93 / 18.5265
        file-print (word "Cal EPA," sown-power)
    ]
    ask turtles with [label = "Cal Water"][
    set sown-power scalepower * 1.07 / 18.5265
        file-print (word "Cal Water," sown-power)
    ]
    ask turtles with [label = "Cal Fish"][
    set sown-power scalepower * 1.3 / 18.5265
        file-print (word "Cal Fish," sown-power)
    ]
    ask turtles with [label = "Cal Park"][
    set sown-power scalepower * 0.96 / 18.5265
        file-print (word "Cal Park," sown-power)
    ]
    ask turtles with [label = "Cal Forest"][
    set sown-power scalepower * 1.26 / 18.5265
        file-print (word "Cal Forest," sown-power)
    ]
    ask turtles with [label = "BLM"][
    set sown-power scalepower * 1.19 / 18.5265
        file-print (word "BLM," sown-power)
    ]
    ask turtles with [label = "Fish & Wildlife"][
    set sown-power scalepower * 0.89 / 18.5265
        file-print (word "Fish & Wildlife," sown-power)
    ]
    ask turtles with [label = "EPA"][
    set sown-power scalepower * 1.04 / 18.5265
        file-print (word "EPA," sown-power)
    ]
    ask turtles with [label = "SCE"][
    set sown-power scalepower * 1.04 / 18.5265
        file-print (word "SCE," sown-power)
    ]
    ask turtles with [label = "NRDC"][
    set sown-power scalepower * 1.11 / 18.5265
        file-print (word "NRDC," sown-power)
    ]
    ask turtles with [label = "Sierra"][
    set sown-power scalepower * 1.11 / 18.5265
        file-print (word "Sierra," sown-power)
    ]
    ask turtles with [label = "CBD"][
    set sown-power scalepower * 1 / 18.5265
        file-print (word "CBD," sown-power)
    ]
    ask turtles with [label = "Cal Vehicle"][
    set sown-power scalepower * 0.67 / 18.5265
        file-print (word "Cat Vehicle," sown-power)
    ]
    ask turtles with [label = "City"][
    set sown-power scalepower * 0.96 / 18.5265
        file-print (word "City," sown-power)
    ]
    ask turtles with [label = "Renewable"][
    set sown-power scalepower * 0.93 / 18.5265
        file-print (word "Renewable," sown-power)
    ]
  ]
  file-close-all
end


to stakeholder-talk
  if ticks = 1 [
    file-open "stakeOutCreate.csv"
    file-print (word "stage,linknum,label1,label2xcor,ycor,spref1,spower1,seu1,xcor,ycor,spref2,spower2,seu2,scbopref,scbopower,scboeu,scboeu1,scboeu2,e1stemp-eu,e2stemp-eu")
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "stage,linknum,label1,label2,xcor1,ycor1,xcor2,ycor2,e1-stemp-eu,e2-stemp-eu,scboeu1,scboeu2,e1-sown-eu,e2-sown-eu")
    file-print (word "equal tick " ticks)
    file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 1
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
     set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 1]
       [set stemp-eu 0]
       [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 1]
       [set stemp-eu 0]
       [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
     set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 1] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1
      ]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2
      ]
      set hidden? FALSE
      file-open "stakeEquals.csv"
      file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
      file-close-all
    ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
    file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
    ]
  ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 2 [
    file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word ",id,e1-xcor,e1-ycor,e2-xcor,e2-ycor,spref1,spower1,seu1,spref2,spower2,seu2,scbopref,scbopower, scboeu, scboeu1,scboeu2")
    file-print (word "less tick " ticks)
    file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 2
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 2]
       [set stemp-eu 0]
              [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 2]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
          set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 2] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
          file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
      ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
                file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 2] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
    file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 2] = 0 and count my-in-links with [cbolink? = 2] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 2] = 0 and count my-in-links with [cbolink? = 2] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 2] = 0 and count my-in-links with [cbolink? = 2] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 2] = 0 and count my-in-links with [cbolink? = 2] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
    file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 3 [
    file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 3
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 3]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 3]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
          set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 3] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
          file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
      ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
    file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 3] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
         file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 3] = 0 and count my-in-links with [cbolink? = 3] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 3] = 0 and count my-in-links with [cbolink? = 3] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 3] = 0 and count my-in-links with [cbolink? = 3] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 3] = 0 and count my-in-links with [cbolink? = 3] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
        file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 4 [
    file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 4
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 4]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 4]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
     set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 4] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
          file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
      ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
    file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 4] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
         file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 4] = 0 and count my-in-links with [cbolink? = 4] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 4] = 0 and count my-in-links with [cbolink? = 4] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 4] = 0 and count my-in-links with [cbolink? = 4] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 4] = 0 and count my-in-links with [cbolink? = 4] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
        file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 5 [
    file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 5
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 5]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 5]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
          set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 5] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
          file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
      ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
    file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 5] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
         file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 5] = 0 and count my-in-links with [cbolink? = 5] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 5] = 0 and count my-in-links with [cbolink? = 5] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 5] = 0 and count my-in-links with [cbolink? = 5] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 5] = 0 and count my-in-links with [cbolink? = 5] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
        file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
     file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 6 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 6
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 6]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 6]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 6] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
      ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]

          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 6] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 6] = 0 and count my-in-links with [cbolink? = 6] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 6] = 0 and count my-in-links with [cbolink? = 6] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 6] = 0 and count my-in-links with [cbolink? = 6] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 6] = 0 and count my-in-links with [cbolink? = 6] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 7 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 7
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 7]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 7]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 7] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
       ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 7] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 7] = 0 and count my-in-links with [cbolink? = 7] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 7] = 0 and count my-in-links with [cbolink? = 7] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 7] = 0 and count my-in-links with [cbolink? = 7] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 7] = 0 and count my-in-links with [cbolink? = 7] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 8 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 8
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 8]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 8]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 8] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
      ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 8] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 8] = 0 and count my-in-links with [cbolink? = 8] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 8] = 0 and count my-in-links with [cbolink? = 8] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 8] = 0 and count my-in-links with [cbolink? = 8] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 8] = 0 and count my-in-links with [cbolink? = 8] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 9 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 9
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 9]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 9]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 9] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
       ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 9] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 9] = 0 and count my-in-links with [cbolink? = 9] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 9] = 0 and count my-in-links with [cbolink? = 9] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 9] = 0 and count my-in-links with [cbolink? = 9] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 9] = 0 and count my-in-links with [cbolink? = 9] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 10 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 10
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 10]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 10]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 10] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
       ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 10] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 10] = 0 and count my-in-links with [cbolink? = 10] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 10] = 0 and count my-in-links with [cbolink? = 10] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 10] = 0 and count my-in-links with [cbolink? = 10] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 10] = 0 and count my-in-links with [cbolink? = 10] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 11 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 11
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 11]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 11]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 11] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
       ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 11] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 11] = 0 and count my-in-links with [cbolink? = 11] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 11] = 0 and count my-in-links with [cbolink? = 11] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 11] = 0 and count my-in-links with [cbolink? = 11] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 11] = 0 and count my-in-links with [cbolink? = 11] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 12 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 12
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 12]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 12]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 12] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
       ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 12] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 12] = 0 and count my-in-links with [cbolink? = 12] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 12] = 0 and count my-in-links with [cbolink? = 12] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 12] = 0 and count my-in-links with [cbolink? = 12] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 12] = 0 and count my-in-links with [cbolink? = 12] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 13 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 13
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 13]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 13]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 13] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
       ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 13] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 13] = 0 and count my-in-links with [cbolink? = 13] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 13] = 0 and count my-in-links with [cbolink? = 13] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 13] = 0 and count my-in-links with [cbolink? = 13] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 13] = 0 and count my-in-links with [cbolink? = 13] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 14 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 14
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 14]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 14]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 14] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
       ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 14] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 14] = 0 and count my-in-links with [cbolink? = 14] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 14] = 0 and count my-in-links with [cbolink? = 14] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 14] = 0 and count my-in-links with [cbolink? = 14] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 14] = 0 and count my-in-links with [cbolink? = 14] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 15 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 15
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 15]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 15]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 15] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
       ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 15] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 15] = 0 and count my-in-links with [cbolink? = 15] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 15] = 0 and count my-in-links with [cbolink? = 15] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 15] = 0 and count my-in-links with [cbolink? = 15] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 15] = 0 and count my-in-links with [cbolink? = 15] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 16 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 16
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 16]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 16]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 16] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
       ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 16] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 16] = 0 and count my-in-links with [cbolink? = 16] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 16] = 0 and count my-in-links with [cbolink? = 16] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 16] = 0 and count my-in-links with [cbolink? = 16] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 16] = 0 and count my-in-links with [cbolink? = 16] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 17 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 17
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 17]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 17]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 17] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
       ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 17] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 17] = 0 and count my-in-links with [cbolink? = 17] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 17] = 0 and count my-in-links with [cbolink? = 17] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 17] = 0 and count my-in-links with [cbolink? = 17] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 17] = 0 and count my-in-links with [cbolink? = 17] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 18 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 18
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 18]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 18]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 18] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
      ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 18] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 18] = 0 and count my-in-links with [cbolink? = 18] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 18] = 0 and count my-in-links with [cbolink? = 18] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 18] = 0 and count my-in-links with [cbolink? = 18] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 18] = 0 and count my-in-links with [cbolink? = 18] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 19 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 19
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 19]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 19]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 19] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
      ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 19] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 19] = 0 and count my-in-links with [cbolink? = 19] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 19] = 0 and count my-in-links with [cbolink? = 19] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 19] = 0 and count my-in-links with [cbolink? = 19] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 19] = 0 and count my-in-links with [cbolink? = 19] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]

  if ticks = 20 [
        file-open "stakeOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "stakeEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    file-open "stakeLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
       file-open "scbopower.csv"
   file-print (word "adjust " ticks)
   file-close-all
  ask turtles with [stakeholder? = 1] [
    create-linkstakeholders-to turtles with [stakeholder? = 1] with [who != [who] of myself]
    [set cbolink? 20
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
          set globstempeu1 scboeu1
     set globstempeu2 scboeu2
     ask end1 [ifelse empty? [scboeu1] of my-out-links with [cbolink? = 20]
       [set stemp-eu 0]        [ if globstempeu1 > stemp-eu [set stemp-eu globstempeu1]]]
     ask end2 [ifelse empty? [scboeu1] of my-in-links with [cbolink? = 20]
       [set stemp-eu 0] [ if globstempeu2 > stemp-eu [set stemp-eu globstempeu2]]]
     set hidden? TRUE
               set sid globslink
     set globslink globslink + 1
      file-open "stakeOutCreate.csv"
      file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," spref1 "," spower1 "," seu1 "," [xcor] of end2 "," [ycor] of end2 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2 "," [stemp-eu] of end1 "," [stemp-eu] of end2)
      file-close-all
     ]
    ]
  ask linkstakeholders with [cbolink? = 20] [
    ;if ([stemp-eu] of end1 <= [sown-eu] of end1) or ([stemp-eu] of end2 <= [sown-eu] of end2)
    ;[die]
    ;if scboeu1 < [stemp-eu] of end1 [die]
    ;if scboeu2 <= [sown-eu] of end2 [die]
    set bool1 precision ([stemp-eu] of end1) 6 > precision ([sown-eu] of end1) 6
    set bool2 precision ([stemp-eu] of end2) 6 > precision ([sown-eu] of end2) 6
    set bool3 bool1 and bool2
            file-open "stakeEquals.csv"
    file-print (word "," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [stemp-eu] of end1 "," [stemp-eu] of end2 "," scboeu1 "," scboeu2 "," [sown-eu] of end1 "," [sown-eu] of end2 "," bool1 "," bool2 "," bool3)
    file-close-all
    if ([stemp-eu] of end1) + ([stemp-eu] of end2) != (scboeu1 + scboeu2)
    [die]
    ;ifelse ([stemp-eu] of end1 > [sown-eu] of end1) and ([stemp-eu] of end2 > [sown-eu] of end2)
    ifelse (bool3)
    [
      set globpref scbopref
      set globpow scbopower
      set globutil1 scboeu1
      set globutil2 scboeu2
      ask end1 [
        set sturcbo? 1
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power globpow
        set sown-eu globutil1
        set scbo-eu globutil1]
      ask end2 [
        set sturcbo? 0
        set sown-pref globpref
        set own-pref sown-pref
        set scbo-pref globpref
        set scbo-power 0
        set sown-eu globutil2
        set scbo-eu globutil2]
      set hidden? FALSE
                file-open "stakeEquals.csv"
    file-print (word "b1," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
      ]
    [ask end1 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
      ask end2 [
        set sown-pref sown-pref
        set own-pref sown-pref
        set rown-pref sown-pref
        set sown-power sown-power]
          file-open "stakeEquals.csv"
    file-print (word "b2," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [sturcbo?] of end1 "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sturcbo?] of end2 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2)
    file-close-all
  ]]
    ask linkstakeholders with [cbolink? < 20] [
     set spref1 [sown-pref] of end1
     set spower1 [sown-power] of end1
     set seu1 [sown-eu] of end1
     set spref2 [sown-pref] of end2
     set spower2 [sown-power] of end2
     set seu2 [sown-eu] of end2
     set scbopref ((spref1 * spower1 + spref2 * spower2)/(spower1 + spower2 + 0.0000001))
     set scbopower spower1 + spower2
     set scboeu scbopower * (100 - abs (scbopref - scbopref))
     set scboeu1 (100 - abs(scbopref - spref1)) * (spower1 + (scbopower - spower1) * (spower1 / (scbopower + 0.000001)))
     set scboeu2 (100 - abs(scbopref - spref2)) * (spower2 + (scbopower - spower2) * (spower2 / (scbopower + 0.000001)))
              file-open "stakeLess.csv"
    file-print (word "eval," sid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," spref1 "," spower1 "," seu1 "," spref2 "," spower2 "," seu2 "," scbopref "," scbopower "," scboeu "," scboeu1 "," scboeu2)
    file-close-all
        ifelse ( (precision (scboeu1) 6) < (precision ([sown-eu] of end1) 6)) or ( (precision (scboeu2) 6) < (precision ([sown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [cbolink? = 20] = 0 and count my-in-links with [cbolink? = 20] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
    ask end2 [
        if count my-out-links with [cbolink? = 20] = 0 and count my-in-links with [cbolink? = 20] = 0 [
        set sturcbo? 0
        set scbo-power 0]]
      die]
    [if [sown-pref] of end1 != [sown-pref] of end2 [
        ask end1 [
        if count my-out-links with [cbolink? = 20] = 0 and count my-in-links with [cbolink? = 20] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]
    ask end2 [
        if count my-out-links with [cbolink? = 20] = 0 and count my-in-links with [cbolink? = 20] = 0 [
        set sturcbo? 1
        set sown-pref [scbo-pref] of other-end
        set rown-pref sown-pref
        set scbo-pref [scbo-pref] of other-end
        set scbo-power 0
        set sown-eu (100 - abs (sown-pref - sown-pref)) * sown-power]]]
            file-open "stakeLess.csv"
    file-print (word "b2," sid "," [sown-pref] of end1 "," [scbo-power] of end1 "," [sown-eu] of end1 "," [sown-pref] of end2 "," [scbo-power] of end2 "," [sown-eu] of end2 "," [sturcbo?] of end1 "," [sturcbo?] of end2)
    file-close-all
    ]
    ]
   ask turtles with [stakeholder? = 1] [
     set scbo-power (sum [scbopower] of my-out-links with [cbolink? > 0]) + (sum [scbopower] of my-in-links with [cbolink? > 0]) - sown-power * ((count my-out-links with [cbolink? > 0]) + (count my-in-links with [cbolink? > 0]) - 1)
          file-open "scbopower.csv"
     file-print (word "," label "," xcor "," ycor ",sumpower," scbo-power "," sum [scbopower] of my-out-links with [cbolink? > 0] "," sum [scbopower] of my-in-links with [cbolink? > 0] "," sown-power "," count my-out-links with [cbolink? > 0] "," count my-in-links with [cbolink? > 0])
     file-close-all
     ]
   ask turtles with [stakeholder? = 1] [if (count my-out-links with [cbolink? >= 1] = 0) and (count my-in-links with [cbolink? >= 1] = 0)
    [set sturcbo? 0
     set scbo-power 0]]
  ]
end

to regulator-setup
  if ticks = 15 [
  set scalepower2 max [sown-power] of turtles with [stakeholder? = 1]
  create-regulators 1 [
    set label "Peevey"
    set label-color black
    set shape "chess king"
    set size 5
    setxy -70 1
    set heading 0
    fd 8
    set color 105
    set rown-pref 10
    set sown-pref rown-pref
    set rown-power scalepower2 * 2 * 0.85
    set sown-power rown-power
    set regulator? 1
    set stakeholder? 1]
  create-regulators 1 [
    set label "Gruenich"
    set label-color black
    set shape "chess king"
    set size 5
    setxy -70 1
    set heading 144
    fd 8
    set color 105
    set rown-pref 1
    set sown-pref rown-pref
    set rown-power scalepower2 * 2 * 1
    set sown-power rown-power
    set regulator? 1
    set stakeholder? 1]
  create-regulators 1 [
    set label "Chong"
    set label-color black
    set shape "chess king"
    set size 5
    setxy -70 1
    set heading 216
    fd 8
    set color 105
    set rown-pref 20
    set sown-pref rown-pref
    set rown-power scalepower2 * 2 * 0.4
    set sown-power rown-power
    set regulator? 1
    set stakeholder? 1]
   create-regulators 1 [
    set label "Bohn"
    set label-color black
    set shape "chess king"
    set size 5
    setxy -70 1
    set heading 288
    fd 8
    set color 105
    set rown-pref 20
    set sown-pref rown-pref
    set rown-power scalepower2 * 2 * 0.4
    set sown-power rown-power
    set regulator? 1
    set stakeholder? 1]
   create-regulators 1 [
    set label "Simon"
    set label-color black
    set shape "chess king"
    set size 5
    setxy -70 1
    set heading 72
    fd 8
    set color 105
    set rown-pref 20
    set sown-pref rown-pref
    set rown-power scalepower2 * 2 * 0.4
    set sown-power rown-power
    set regulator? 1
    set stakeholder? 1]]

   if ticks > 15 [
  set scalepower2 max [sown-power] of turtles with [stakeholder? = 1]
    ask turtles with [label = "Peevey"][
    set rown-power scalepower * 0.85  * 2
    set sown-power rown-power]
    ask turtles with [label = "Gruenich"][
    set rown-power scalepower * 1 * 2
    set sown-power rown-power]
    ask turtles with [label = "Chong"][
    set rown-power scalepower * 0.4 * 2
    set sown-power rown-power]
    ask turtles with [label = "Bohn"][
    set rown-power scalepower * 0.4 * 2
    set sown-power rown-power]
    ask turtles with [label = "Simon"][
    set rown-power scalepower * 0.4 * 2
    set sown-power rown-power]
  ]
end

to regulator-talk
  if ticks = 21 [
            file-open "regOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "regEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
    ask turtles with [regulator? = 1] [
      create-linkregulators-to regulators with [who != [who] of myself]
    [set regulatorlink? 1
     set rpref1 [rown-pref] of end1
     set rpower1 [rown-power] of end1
     set reu1 [rown-eu] of end1
     set rpref2 [rown-pref] of end2
     set rpower2 [rown-power] of end2
     set reu2 [rown-eu] of end2
     set rcbopref ((rpref1 * rpower1 + rpref2 * rpower2)/(rpower1 + rpower2 + 0.0000001))
     set rcbopower rpower1 + rpower2
     set rcboeu rcbopower * (100 - abs (rcbopref - rcbopref))
     set rcboeu1 (100 - abs(rcbopref - rpref1)) * (rpower1 + (rcbopower - rpower1) * (rpower1 / (rcbopower + 0.000001)))
     set rcboeu2 (100 - abs(rcbopref - rpref2)) * (rpower2 + (rcbopower - rpower2) * (rpower2 / (rcbopower + 0.000001)))
     set globrtempeu1 rcboeu1
     set globrtempeu2 rcboeu2
     ask end1 [ifelse empty? [rcboeu1] of my-out-links with [regulatorlink? = 1]
       [set rtemp-eu 0] [ if globrtempeu1 > rtemp-eu [set rtemp-eu globrtempeu1]]]
     ask end2 [ifelse empty? [rcboeu1] of my-in-links with [regulatorlink? = 1]
       [set rtemp-eu 0] [ if globrtempeu2 > rtemp-eu [set rtemp-eu globrtempeu2]]]
     set hidden? TRUE
     set rid globrlink
     set globrlink globrlink + 1
      file-open "regOutCreate.csv"
      file-print (word "," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," rpref1 "," rpower1 "," reu1 "," [xcor] of end2 "," [ycor] of end2 "," rpref2 "," rpower2 "," reu2 "," rcbopref "," rcbopower "," rcboeu "," rcboeu1 "," rcboeu2 "," [rtemp-eu] of end1 "," [rtemp-eu] of end2)
      file-close-all
     ]]
    ask linkregulators with [regulatorlink? = 1] [
    file-open "regEquals.csv"
    file-print (word "," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [rtemp-eu] of end1 "," [rtemp-eu] of end2 "," rcboeu1 "," rcboeu2 "," [rown-eu] of end1 "," [rown-eu] of end2)
    file-close-all
    if ([rtemp-eu] of end1) + ([rtemp-eu] of end2) != (rcboeu1 + rcboeu2)
    [die]
    ifelse ((precision ([rtemp-eu] of end1) 6) > (precision ([rown-eu] of end1) 6)) and ( (precision ([rtemp-eu] of end2) 6) > (precision ([rown-eu] of end2) 6))
    [
      set globpref rcbopref
      set globpow rcbopower
      set globutil1 rcboeu1
      set globutil2 rcboeu2
      ask end1 [
        set rturcbo? 1
        set rown-pref globpref
        set own-pref rown-pref
        set rcbo-pref globpref
        set rcbo-power globpow
        set rown-eu globutil1
        set rcbo-eu globutil1]
      ask end2 [
        set rturcbo? 0
        set rown-pref [rcbo-pref] of other-end
        set own-pref sown-pref
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu globutil2
        set rcbo-eu globutil2]
      set hidden? FALSE ]
    [ask end1 [
        set rown-pref rown-pref
        set own-pref rown-pref
        set rown-power rown-power]
      ask end2 [
        set rown-pref rown-pref
        set own-pref rown-pref
        set rown-power rown-power]]]
]

  if ticks = 22 [
                file-open "regOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "regEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
        file-close-all
        file-open "regLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
    ask turtles with [regulator? = 1] [
      create-linkregulators-to regulators with [who != [who] of myself]
    [set regulatorlink? 2
     set rpref1 [rown-pref] of end1
     set rpower1 [rown-power] of end1
     set reu1 [rown-eu] of end1
     set rpref2 [rown-pref] of end2
     set rpower2 [rown-power] of end2
     set reu2 [rown-eu] of end2
     set rcbopref ((rpref1 * rpower1 + rpref2 * rpower2)/(rpower1 + rpower2 + 0.0000001))
     set rcbopower rpower1 + rpower2
     set rcboeu rcbopower * (100 - abs (rcbopref - rcbopref))
     set rcboeu1 (100 - abs(rcbopref - rpref1)) * (rpower1 + (rcbopower - rpower1) * (rpower1 / (rcbopower + 0.000001)))
     set rcboeu2 (100 - abs(rcbopref - rpref2)) * (rpower2 + (rcbopower - rpower2) * (rpower2 / (rcbopower + 0.000001)))
          set globrtempeu1 rcboeu1
     set globrtempeu2 rcboeu2
     ask end1 [ifelse empty? [rcboeu1] of my-out-links with [regulatorlink? = 2]
       [set rtemp-eu 0] [ if globrtempeu1 > rtemp-eu [set rtemp-eu globrtempeu1]]]
     ask end2 [ifelse empty? [rcboeu1] of my-in-links with [regulatorlink? = 2]
       [set rtemp-eu 0] [ if globrtempeu2 > rtemp-eu [set rtemp-eu globrtempeu2]]]
     set hidden? TRUE
          set rid globrlink
     set globrlink globrlink + 1
      file-open "regOutCreate.csv"
      file-print (word "," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," rpref1 "," rpower1 "," reu1 "," [xcor] of end2 "," [ycor] of end2 "," rpref2 "," rpower2 "," reu2 "," rcbopref "," rcbopower "," rcboeu "," rcboeu1 "," rcboeu2 "," [rtemp-eu] of end1 "," [rtemp-eu] of end2)
      file-close-all
     ]]
    ask linkregulators with [regulatorlink? = 2] [
         file-open "regEquals.csv"
    file-print (word "," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [rtemp-eu] of end1 "," [rtemp-eu] of end2 "," rcboeu1 "," rcboeu2 "," [rown-eu] of end1 "," [rown-eu] of end2)
    file-close-all
    if ([rtemp-eu] of end1) + ([rtemp-eu] of end2) != (rcboeu1 + rcboeu2)
    [die]
    ifelse ((precision ([rtemp-eu] of end1) 6) > (precision ([rown-eu] of end1) 6)) and ( (precision ([rtemp-eu] of end2) 6) > (precision ([rown-eu] of end2) 6))
    [
      set globpref rcbopref
      set globpow rcbopower
      set globutil1 rcboeu1
      set globutil2 rcboeu2
      ask end1 [
        set rturcbo? 1
        set rown-pref globpref
        set own-pref rown-pref
        set rcbo-pref globpref
        set rcbo-power globpow
        set rown-eu globutil1
        set rcbo-eu globutil1]
      ask end2 [
        set rturcbo? 0
        set rown-pref [rcbo-pref] of other-end
        set own-pref sown-pref
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu globutil2
        set rcbo-eu globutil2]
      set hidden? FALSE ]
    [ask end1 [
        set rown-pref rown-pref
        set own-pref rown-pref
        set rown-power rown-power]
      ask end2 [
        set rown-pref rown-pref
        set own-pref rown-pref
        set rown-power rown-power]]]
    ask linkregulators with [regulatorlink? < 2] [
     set rpref1 [rown-pref] of end1
     set rpower1 [rown-power] of end1
     set reu1 [rown-eu] of end1
     set rpref2 [rown-pref] of end2
     set rpower2 [rown-power] of end2
     set reu2 [rown-eu] of end2
     set rcbopref ((rpref1 * rpower1 + rpref2 * rpower2)/(rpower1 + rpower2 + 0.0000001))
     set rcbopower rpower1 + rpower2
     set rcboeu rcbopower * (100 - abs (rcbopref - rcbopref))
     set rcboeu1 (100 - abs(rcbopref - rpref1)) * (rpower1 + (rcbopower - rpower1) * (rpower1 / (rcbopower + 0.000001)))
     set rcboeu2 (100 - abs(rcbopref - rpref2)) * (rpower2 + (rcbopower - rpower2) * (rpower2 / (rcbopower + 0.000001)))
    file-open "regLess.csv"
    file-print (word "eval," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," rpref1 "," rpower1 "," reu1 "," rpref2 "," rpower2 "," reu2 "," rcbopref "," rcbopower "," rcboeu "," rcboeu1 "," rcboeu2)
    file-close-all
    ifelse ((precision rcboeu1 6) < (precision ([rown-eu] of end1) 6)) or ((precision rcboeu2 6) < (precision ([rown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [regulatorlink? = 2] = 0 and count my-in-links with [regulatorlink? = 2] = 0 [
        set rturcbo? 0
        set rcbo-power 0]]
    ask end2 [
        if count my-out-links with [regulatorlink? = 2] = 0 and count my-in-links with [regulatorlink? = 2] = 0 [
        set rturcbo? 0
        set rcbo-power 0]]
      die]
    [if [rown-pref] of end1 != [rown-pref] of end2 [
        ask end1 [
        if count my-out-links with [regulatorlink? = 2] = 0 and count my-in-links with [regulatorlink? = 2] = 0 [
        set rturcbo? 1
        set rown-pref [rcbo-pref] of other-end
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu (100 - abs (rown-pref - rown-pref)) * rown-power]]
    ask end2 [
        if count my-out-links with [regulatorlink? = 2] = 0 and count my-in-links with [regulatorlink? = 2] = 0 [
        set rturcbo? 1
        set rown-pref [rcbo-pref] of other-end
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu (100 - abs (rown-pref - rown-pref)) * rown-power]]]
    ]
    ]
   ask turtles with [regulator? = 1] [
     set rcbo-power (sum [rcbopower] of my-out-links with [regulatorlink? > 0]) + (sum [rcbopower] of my-in-links with [regulatorlink? > 0]) - rown-power * ((count my-out-links with [regulatorlink? > 0]) + (count my-in-links with [regulatorlink? > 0]) - 1)]
   ask turtles with [regulator? = 1] [if (count my-out-links with [regulatorlink? >= 1] = 0) and (count my-in-links with [regulatorlink? >= 1] = 0)
    [set rturcbo? 0
     set rcbo-power 0]]
]

  if ticks = 23 [
                file-open "regOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "regEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
            file-open "regLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
    ask turtles with [regulator? = 1] [
      create-linkregulators-to regulators with [who != [who] of myself]
    [set regulatorlink? 3
     set rpref1 [rown-pref] of end1
     set rpower1 [rown-power] of end1
     set reu1 [rown-eu] of end1
     set rpref2 [rown-pref] of end2
     set rpower2 [rown-power] of end2
     set reu2 [rown-eu] of end2
     set rcbopref ((rpref1 * rpower1 + rpref2 * rpower2)/(rpower1 + rpower2 + 0.0000001))
     set rcbopower rpower1 + rpower2
     set rcboeu rcbopower * (100 - abs (rcbopref - rcbopref))
     set rcboeu1 (100 - abs(rcbopref - rpref1)) * (rpower1 + (rcbopower - rpower1) * (rpower1 / (rcbopower + 0.000001)))
     set rcboeu2 (100 - abs(rcbopref - rpref2)) * (rpower2 + (rcbopower - rpower2) * (rpower2 / (rcbopower + 0.000001)))
          set globrtempeu1 rcboeu1
     set globrtempeu2 rcboeu2
     ask end1 [ifelse empty? [rcboeu1] of my-out-links with [regulatorlink? = 3]
       [set rtemp-eu 0] [ if globrtempeu1 > rtemp-eu [set rtemp-eu globrtempeu1]]]
     ask end2 [ifelse empty? [rcboeu1] of my-in-links with [regulatorlink? = 3]
       [set rtemp-eu 0] [ if globrtempeu2 > rtemp-eu [set rtemp-eu globrtempeu2]]]
     set hidden? TRUE
          set rid globrlink
     set globrlink globrlink + 1
      file-open "regOutCreate.csv"
      file-print (word "," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," rpref1 "," rpower1 "," reu1 "," [xcor] of end2 "," [ycor] of end2 "," rpref2 "," rpower2 "," reu2 "," rcbopref "," rcbopower "," rcboeu "," rcboeu1 "," rcboeu2 "," [rtemp-eu] of end1 "," [rtemp-eu] of end2)
      file-close-all
     ]]
    ask linkregulators with [regulatorlink? = 3] [
         file-open "regEquals.csv"
    file-print (word "," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [rtemp-eu] of end1 "," [rtemp-eu] of end2 "," rcboeu1 "," rcboeu2 "," [rown-eu] of end1 "," [rown-eu] of end2)
    file-close-all
    if ([rtemp-eu] of end1) + ([rtemp-eu] of end2) != (rcboeu1 + rcboeu2)
    [die]
    ifelse ((precision ([rtemp-eu] of end1) 6) > (precision ([rown-eu] of end1) 6)) and ( (precision ([rtemp-eu] of end2) 6) > (precision ([rown-eu] of end2) 6))
    [
      set globpref rcbopref
      set globpow rcbopower
      set globutil1 rcboeu1
      set globutil2 rcboeu2
      ask end1 [
        set rturcbo? 1
        set rown-pref globpref
        set own-pref rown-pref
        set rcbo-pref globpref
        set rcbo-power globpow
        set rown-eu globutil1
        set rcbo-eu globutil1]
      ask end2 [
        set rturcbo? 0
        set rown-pref [rcbo-pref] of other-end
        set own-pref sown-pref
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu globutil2
        set rcbo-eu globutil2]
      set hidden? FALSE ]
    [ask end1 [
        set rown-pref rown-pref
        set own-pref rown-pref
        set rown-power rown-power]
      ask end2 [
        set rown-pref rown-pref
        set own-pref rown-pref
        set rown-power rown-power]]]
    ask linkregulators with [regulatorlink? < 3] [
     set rpref1 [rown-pref] of end1
     set rpower1 [rown-power] of end1
     set reu1 [rown-eu] of end1
     set rpref2 [rown-pref] of end2
     set rpower2 [rown-power] of end2
     set reu2 [rown-eu] of end2
     set rcbopref ((rpref1 * rpower1 + rpref2 * rpower2)/(rpower1 + rpower2 + 0.0000001))
     set rcbopower rpower1 + rpower2
     set rcboeu rcbopower * (100 - abs (rcbopref - rcbopref))
     set rcboeu1 (100 - abs(rcbopref - rpref1)) * (rpower1 + (rcbopower - rpower1) * (rpower1 / (rcbopower + 0.000001)))
     set rcboeu2 (100 - abs(rcbopref - rpref2)) * (rpower2 + (rcbopower - rpower2) * (rpower2 / (rcbopower + 0.000001)))
         file-open "regLess.csv"
    file-print (word "eval," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," rpref1 "," rpower1 "," reu1 "," rpref2 "," rpower2 "," reu2 "," rcbopref "," rcbopower "," rcboeu "," rcboeu1 "," rcboeu2)
    file-close-all
    ifelse ((precision rcboeu1 6) < (precision ([rown-eu] of end1) 6)) or ((precision rcboeu2 6) < (precision ([rown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [regulatorlink? = 3] = 0 and count my-in-links with [regulatorlink? = 3] = 0 [
        set rturcbo? 0
        set rcbo-power 0]]
    ask end2 [
        if count my-out-links with [regulatorlink? = 3] = 0 and count my-in-links with [regulatorlink? = 3] = 0 [
        set rturcbo? 0
        set rcbo-power 0]]
      die]
    [if [rown-pref] of end1 != [rown-pref] of end2 [
        ask end1 [
        if count my-out-links with [regulatorlink? = 3] = 0 and count my-in-links with [regulatorlink? = 3] = 0 [
        set rturcbo? 1
        set rown-pref [rcbo-pref] of other-end
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu (100 - abs (rown-pref - rown-pref)) * rown-power]]
    ask end2 [
        if count my-out-links with [regulatorlink? = 3] = 0 and count my-in-links with [regulatorlink? = 3] = 0 [
        set rturcbo? 1
        set rown-pref [rcbo-pref] of other-end
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu (100 - abs (rown-pref - rown-pref)) * rown-power]]]
    ]
    ]
   ask turtles with [regulator? = 1] [
     set rcbo-power (sum [rcbopower] of my-out-links with [regulatorlink? > 0]) + (sum [rcbopower] of my-in-links with [regulatorlink? > 0]) - rown-power * ((count my-out-links with [regulatorlink? > 0]) + (count my-in-links with [regulatorlink? > 0]) - 1)]
   ask turtles with [regulator? = 1] [if (count my-out-links with [regulatorlink? >= 1] = 0) and (count my-in-links with [regulatorlink? >= 1] = 0)
    [set rturcbo? 0
     set rcbo-power 0]]
]

   if ticks = 24 [
                 file-open "regOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "regEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
            file-open "regLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
    ask turtles with [regulator? = 1] [
      create-linkregulators-to regulators with [who != [who] of myself]
    [set regulatorlink? 4
     set rpref1 [rown-pref] of end1
     set rpower1 [rown-power] of end1
     set reu1 [rown-eu] of end1
     set rpref2 [rown-pref] of end2
     set rpower2 [rown-power] of end2
     set reu2 [rown-eu] of end2
     set rcbopref ((rpref1 * rpower1 + rpref2 * rpower2)/(rpower1 + rpower2 + 0.0000001))
     set rcbopower rpower1 + rpower2
     set rcboeu rcbopower * (100 - abs (rcbopref - rcbopref))
     set rcboeu1 (100 - abs(rcbopref - rpref1)) * (rpower1 + (rcbopower - rpower1) * (rpower1 / (rcbopower + 0.000001)))
     set rcboeu2 (100 - abs(rcbopref - rpref2)) * (rpower2 + (rcbopower - rpower2) * (rpower2 / (rcbopower + 0.000001)))
          set globrtempeu1 rcboeu1
     set globrtempeu2 rcboeu2
     ask end1 [ifelse empty? [rcboeu1] of my-out-links with [regulatorlink? = 4]
       [set rtemp-eu 0] [ if globrtempeu1 > rtemp-eu [set rtemp-eu globrtempeu1]]]
     ask end2 [ifelse empty? [rcboeu1] of my-in-links with [regulatorlink? = 4]
       [set rtemp-eu 0] [ if globrtempeu2 > rtemp-eu [set rtemp-eu globrtempeu2]]]
     set hidden? TRUE
          set rid globrlink
     set globrlink globrlink + 1
      file-open "regOutCreate.csv"
      file-print (word "," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," rpref1 "," rpower1 "," reu1 "," [xcor] of end2 "," [ycor] of end2 "," rpref2 "," rpower2 "," reu2 "," rcbopref "," rcbopower "," rcboeu "," rcboeu1 "," rcboeu2 "," [rtemp-eu] of end1 "," [rtemp-eu] of end2)
      file-close-all
     ]]
    ask linkregulators with [regulatorlink? = 4] [
         file-open "regEquals.csv"
    file-print (word "," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [rtemp-eu] of end1 "," [rtemp-eu] of end2 "," rcboeu1 "," rcboeu2 "," [rown-eu] of end1 "," [rown-eu] of end2)
    file-close-all
    if ([rtemp-eu] of end1) + ([rtemp-eu] of end2) != (rcboeu1 + rcboeu2)
    [die]
    ifelse ((precision ([rtemp-eu] of end1) 6) > (precision ([rown-eu] of end1) 6)) and ( (precision ([rtemp-eu] of end2) 6) > (precision ([rown-eu] of end2) 6))
    [
      set globpref rcbopref
      set globpow rcbopower
      set globutil1 rcboeu1
      set globutil2 rcboeu2
      ask end1 [
        set rturcbo? 1
        set rown-pref globpref
        set own-pref rown-pref
        set rcbo-pref globpref
        set rcbo-power globpow
        set rown-eu globutil1
        set rcbo-eu globutil1]
      ask end2 [
        set rturcbo? 0
        set rown-pref [rcbo-pref] of other-end
        set own-pref sown-pref
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu globutil2
        set rcbo-eu globutil2]
      set hidden? FALSE ]
    [ask end1 [
        set rown-pref rown-pref
        set own-pref rown-pref
        set rown-power rown-power]
      ask end2 [
        set rown-pref rown-pref
        set own-pref rown-pref
        set rown-power rown-power]]]
    ask linkregulators with [regulatorlink? < 4] [
     set rpref1 [rown-pref] of end1
     set rpower1 [rown-power] of end1
     set reu1 [rown-eu] of end1
     set rpref2 [rown-pref] of end2
     set rpower2 [rown-power] of end2
     set reu2 [rown-eu] of end2
     set rcbopref ((rpref1 * rpower1 + rpref2 * rpower2)/(rpower1 + rpower2 + 0.0000001))
     set rcbopower rpower1 + rpower2
     set rcboeu rcbopower * (100 - abs (rcbopref - rcbopref))
     set rcboeu1 (100 - abs(rcbopref - rpref1)) * (rpower1 + (rcbopower - rpower1) * (rpower1 / (rcbopower + 0.000001)))
     set rcboeu2 (100 - abs(rcbopref - rpref2)) * (rpower2 + (rcbopower - rpower2) * (rpower2 / (rcbopower + 0.000001)))
         file-open "regLess.csv"
    file-print (word "eval," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," rpref1 "," rpower1 "," reu1 "," rpref2 "," rpower2 "," reu2 "," rcbopref "," rcbopower "," rcboeu "," rcboeu1 "," rcboeu2)
    file-close-all
    ifelse ((precision rcboeu1 6) < (precision ([rown-eu] of end1) 6)) or ((precision rcboeu2 6) < (precision ([rown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [regulatorlink? = 4] = 0 and count my-in-links with [regulatorlink? = 4] = 0 [
        set rturcbo? 0
        set rcbo-power 0]]
    ask end2 [
        if count my-out-links with [regulatorlink? = 4] = 0 and count my-in-links with [regulatorlink? = 4] = 0 [
        set rturcbo? 0
        set rcbo-power 0]]
      die]
    [if [rown-pref] of end1 != [rown-pref] of end2 [
        ask end1 [
        if count my-out-links with [regulatorlink? = 4] = 0 and count my-in-links with [regulatorlink? = 4] = 0 [
        set rturcbo? 1
        set rown-pref [rcbo-pref] of other-end
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu (100 - abs (rown-pref - rown-pref)) * rown-power]]
    ask end2 [
        if count my-out-links with [regulatorlink? = 4] = 0 and count my-in-links with [regulatorlink? = 4] = 0 [
        set rturcbo? 1
        set rown-pref [rcbo-pref] of other-end
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu (100 - abs (rown-pref - rown-pref)) * rown-power]]]
    ]
    ]
   ask turtles with [regulator? = 1] [
     set rcbo-power (sum [rcbopower] of my-out-links with [regulatorlink? > 0]) + (sum [rcbopower] of my-in-links with [regulatorlink? > 0]) - rown-power * ((count my-out-links with [regulatorlink? > 0]) + (count my-in-links with [regulatorlink? > 0]) - 1)]
   ask turtles with [regulator? = 1] [if (count my-out-links with [regulatorlink? >= 1] = 0) and (count my-in-links with [regulatorlink? >= 1] = 0)
    [set rturcbo? 0
     set rcbo-power 0]]
]

    if ticks = 25 [
                  file-open "regOutCreate.csv"
    file-print (word "create tick " ticks)
    file-close-all
    file-open "regEquals.csv"
    file-print (word "equal tick " ticks)
    file-close-all
            file-open "regLess.csv"
    file-print (word "less tick " ticks)
    file-close-all
    ask turtles with [regulator? = 1] [
      create-linkregulators-to regulators with [who != [who] of myself]
    [set regulatorlink? 5
     set rpref1 [rown-pref] of end1
     set rpower1 [rown-power] of end1
     set reu1 [rown-eu] of end1
     set rpref2 [rown-pref] of end2
     set rpower2 [rown-power] of end2
     set reu2 [rown-eu] of end2
     set rcbopref ((rpref1 * rpower1 + rpref2 * rpower2)/(rpower1 + rpower2 + 0.0000001))
     set rcbopower rpower1 + rpower2
     set rcboeu rcbopower * (100 - abs (rcbopref - rcbopref))
     set rcboeu1 (100 - abs(rcbopref - rpref1)) * (rpower1 + (rcbopower - rpower1) * (rpower1 / (rcbopower + 0.000001)))
     set rcboeu2 (100 - abs(rcbopref - rpref2)) * (rpower2 + (rcbopower - rpower2) * (rpower2 / (rcbopower + 0.000001)))
          set globrtempeu1 rcboeu1
     set globrtempeu2 rcboeu2
     ask end1 [ifelse empty? [rcboeu1] of my-out-links with [regulatorlink? = 5]
       [set rtemp-eu 0] [ if globrtempeu1 > rtemp-eu [set rtemp-eu globrtempeu1]]]
     ask end2 [ifelse empty? [rcboeu1] of my-in-links with [regulatorlink? = 5]
       [set rtemp-eu 0] [ if globrtempeu2 > rtemp-eu [set rtemp-eu globrtempeu2]]]
     set hidden? TRUE
          set rid globrlink
     set globrlink globrlink + 1
      file-open "regOutCreate.csv"
      file-print (word "," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," rpref1 "," rpower1 "," reu1 "," [xcor] of end2 "," [ycor] of end2 "," rpref2 "," rpower2 "," reu2 "," rcbopref "," rcbopower "," rcboeu "," rcboeu1 "," rcboeu2 "," [rtemp-eu] of end1 "," [rtemp-eu] of end2)
      file-close-all
     ]]
    ask linkregulators with [regulatorlink? = 5] [
         file-open "regEquals.csv"
    file-print (word "," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," [rtemp-eu] of end1 "," [rtemp-eu] of end2 "," rcboeu1 "," rcboeu2 "," [rown-eu] of end1 "," [rown-eu] of end2)
    file-close-all
    if ([rtemp-eu] of end1) + ([rtemp-eu] of end2) != (rcboeu1 + rcboeu2)
    [die]
    ifelse ((precision ([rtemp-eu] of end1) 6) > (precision ([rown-eu] of end1) 6)) and ( (precision ([rtemp-eu] of end2) 6) > (precision ([rown-eu] of end2) 6))
    [
      set globpref rcbopref
      set globpow rcbopower
      set globutil1 rcboeu1
      set globutil2 rcboeu2
      ask end1 [
        set rturcbo? 1
        set rown-pref globpref
        set own-pref rown-pref
        set rcbo-pref globpref
        set rcbo-power globpow
        set rown-eu globutil1
        set rcbo-eu globutil1]
      ask end2 [
        set rturcbo? 0
        set rown-pref [rcbo-pref] of other-end
        set own-pref sown-pref
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu globutil2
        set rcbo-eu globutil2]
      set hidden? FALSE ]
    [ask end1 [
        set rown-pref rown-pref
        set own-pref rown-pref
        set rown-power rown-power]
      ask end2 [
        set rown-pref rown-pref
        set own-pref rown-pref
        set rown-power rown-power]]]
    ask linkregulators with [regulatorlink? < 5] [
     set rpref1 [rown-pref] of end1
     set rpower1 [rown-power] of end1
     set reu1 [rown-eu] of end1
     set rpref2 [rown-pref] of end2
     set rpower2 [rown-power] of end2
     set reu2 [rown-eu] of end2
     set rcbopref ((rpref1 * rpower1 + rpref2 * rpower2)/(rpower1 + rpower2 + 0.0000001))
     set rcbopower rpower1 + rpower2
     set rcboeu rcbopower * (100 - abs (rcbopref - rcbopref))
     set rcboeu1 (100 - abs(rcbopref - rpref1)) * (rpower1 + (rcbopower - rpower1) * (rpower1 / (rcbopower + 0.000001)))
     set rcboeu2 (100 - abs(rcbopref - rpref2)) * (rpower2 + (rcbopower - rpower2) * (rpower2 / (rcbopower + 0.000001)))
         file-open "regLess.csv"
    file-print (word "eval," rid "," [label] of end1 "," [label] of end2 "," [xcor] of end1 "," [ycor] of end1 "," [xcor] of end2 "," [ycor] of end2 "," rpref1 "," rpower1 "," reu1 "," rpref2 "," rpower2 "," reu2 "," rcbopref "," rcbopower "," rcboeu "," rcboeu1 "," rcboeu2)
    file-close-all
    ifelse ((precision rcboeu1 6) < (precision ([rown-eu] of end1) 6)) or ((precision rcboeu2 6) < (precision ([rown-eu] of end2) 6))
    [ask end1 [
        if count my-out-links with [regulatorlink? = 5] = 0 and count my-in-links with [regulatorlink? = 5] = 0 [
        set rturcbo? 0
        set rcbo-power 0]]
    ask end2 [
        if count my-out-links with [regulatorlink? = 5] = 0 and count my-in-links with [regulatorlink? = 5] = 0 [
        set rturcbo? 0
        set rcbo-power 0]]
      die]
    [if [rown-pref] of end1 != [rown-pref] of end2 [
        ask end1 [
        if count my-out-links with [regulatorlink? = 5] = 0 and count my-in-links with [regulatorlink? = 5] = 0 [
        set rturcbo? 1
        set rown-pref [rcbo-pref] of other-end
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu (100 - abs (rown-pref - rown-pref)) * rown-power]]
    ask end2 [
        if count my-out-links with [regulatorlink? = 5] = 0 and count my-in-links with [regulatorlink? = 5] = 0 [
        set rturcbo? 1
        set rown-pref [rcbo-pref] of other-end
        set rcbo-pref [rcbo-pref] of other-end
        set rcbo-power 0
        set rown-eu (100 - abs (rown-pref - rown-pref)) * rown-power]]]
    ]
    ]
   ask turtles with [regulator? = 1] [
     set rcbo-power (sum [rcbopower] of my-out-links with [regulatorlink? > 0]) + (sum [rcbopower] of my-in-links with [regulatorlink? > 0]) - rown-power * ((count my-out-links with [regulatorlink? > 0]) + (count my-in-links with [regulatorlink? > 0]) - 1)]
   ask turtles with [regulator? = 1] [if (count my-out-links with [regulatorlink? >= 1] = 0) and (count my-in-links with [regulatorlink? >= 1] = 0)
    [set rturcbo? 0
     set rcbo-power 0]]
]
end

;The utility message is received according to random chance and the ideology of the agent.
;The more positive the agent's attitude, the more likely it is to accept the utility's message.
;If the random number is higher than the agent's attitude, the agent becomes more disposed to the utility position by a random amount.
;If the random number is lower, there is a chance the agent will be "turned off" by the utility and become more opposed.

to utility-info
  if ticks >= 1 [
    ifelse Need >= 5 [
  set utilitypref max [sown-pref] of turtles with [label = "SCE"]
  ifelse Utility-Message > Need [
   ask cits [
     if own-pref > utilitypref [
       set idatt (1 + random-float 0.15) * (idatt + Utility-Message * 10 / abs (own-pref - utilitypref))]
     if own-pref < utilitypref [
       set idatt (1 - random-float 0.15) * (idatt + Utility-Message * 10 / abs (own-pref - utilitypref))]
     ]]
  [ask cits [
      if own-pref > utilitypref [
       set idatt (1 - random-float 0.15) * (idatt + Utility-Message * 10 / abs (own-pref - utilitypref))]
      if own-pref < utilitypref [
       set idatt (1 + random-float 0.15) * (idatt + Utility-Message * 10 / abs (own-pref - utilitypref))]
     ]]]
    [ask cits [
        set idatt (1 + random-float 0.05) * (idatt + NGO-Message * 0.01)]]
  ]
end

;The mechanics for the NGO's message mimic those of the utility.
;If the random number is less than the attitude, augmented by the madcount, it accepts the message.
;If it is more, the madcount is increased.

to big-ngo
  if ticks >= 1 [
    ifelse Procedure >= 5 [
  set ngopref1 max [sown-pref] of turtles with [label = "Sierra"]
  set ngopref2 max [sown-pref] of turtles with [label = "NRDC"]
  set ngopref (ngopref1 + ngopref2) / 2
  ifelse NGO-Message > Procedure [
   ask cits [
     if own-pref > ngopref [
       set idatt (1 + random-float 0.15) * (idatt + NGO-Message * 10 / abs (own-pref - ngopref))]
     if own-pref < ngopref [
       set idatt (1 - random-float 0.15) * (idatt + NGO-Message * 10 / abs (own-pref - ngopref))]
     ]]
  [ask cits [
      if own-pref > ngopref [
       set idatt (1 - random-float 0.15) * (idatt + NGO-Message * 10 / abs (own-pref - ngopref))]
      if own-pref < ngopref [
       set idatt (1 + random-float 0.15) * (idatt + NGO-Message * 10 / abs (own-pref - ngopref))]
     ]]]
     
    [ask cits [
        set idatt (1 + random-float 0.05) * (idatt + NGO-Message * 0.01)]]
  ]
end

to merge-cbo
  if ticks = 1 [
  if count cits with [turcbo = 2 and cbo-pref <= 50] != 0 [
  create-turtles 1 [
    set shape "face happy"
    set label "Pro-Development CBO"
    set label-color black
    setxy -70 -50
    set own-power sum [cbo-power] of cits with [turcbo = 2 and cbo-pref <= 50]
    set own-pref (sum [cbo-eu] of cits with [turcbo = 2 and cbo-pref <= 50])/(sum [cbo-power] of cits with [turcbo = 2 and cbo-pref <= 50] + 1)
    set color 105
 ;   set size (count cits with [turcbo = 2 and cbo-pref <= 50])/ 5
    set size 3
    ]]
  if count cits with [turcbo = 2 and cbo-pref > 50] != 0 [
  create-turtles 1 [
    set shape "face sad"
    set label "Anti-Development CBO"
    set label-color black
    setxy -5 -50
    set own-power sum [cbo-power] of cits with [turcbo = 2 and cbo-pref > 50]
    set own-pref (sum [cbo-eu] of cits with [turcbo = 2 and cbo-pref > 50])/(sum [cbo-power] of cits with [turcbo = 2 and cbo-pref > 50])
    set color 15
 ;   set size (count cits with [turcbo = 2 and cbo-pref > 50])/ 5
    set size 3
   ]]]
  if ticks > 1 and ticks <= 20 [
    ask cits with [label = "Pro-Development CBO"] [
    set own-power sum [cbo-power] of cits with [turcbo = 2 and cbo-pref <= 50]
    set own-pref (sum [cbo-eu] of cits with [turcbo = 2 and cbo-pref <= 50])/(sum [cbo-power] of cits with [turcbo = 2 and cbo-pref <= 50] + 1)
    set color 105
;    set size (count cits with [turcbo = 2 and cbo-pref <= 50])/ 5]
    set size 3
    ask cits with [label = "Anti-Development CBO"] [
    set own-power sum [cbo-power] of cits with [turcbo = 2 and cbo-pref > 50]
    set own-pref (sum [cbo-eu] of cits with [turcbo = 2 and cbo-pref > 50])/(sum [cbo-power] of cits with [turcbo = 2 and cbo-pref > 50] + 1)
    set color 105
    set size (count cits with [turcbo = 2 and cbo-pref > 50])/ 5]]]
;  if ticks = 19 [
;    ask cits with [label = "Pro-Development CBO"] [
;      set hidden? true
;      die]
;    ask cits with [label = "Anti-Development CBO"] [
;      set hidden? true
;      die]]

  if ticks = 20 [
;  if count cits with [turcbo = 2 and cbo-pref <= 50] != 0 [
;  create-turtles 1 [
;    set shape "face happy"
;    set label "Pro-Development CBO"
;    set label-color black
;    set stakeholder? 1
;    setxy -70 -50
;    set own-power sum [cbo-power] of cits with [turcbo = 2 and cbo-pref <= 50]
;    set sown-power own-power
;    set own-pref (sum [cbo-eu] of cits with [turcbo = 2 and cbo-pref <= 50])/(sum [cbo-power] of cits with [turcbo = 2 and cbo-pref <= 50] + 1)
;    set sown-pref own-pref
;    set color 105
;    set size (count cits with [turcbo = 2 and cbo-pref <= 50])/ 5
;    ]]
;  if count cits with [turcbo = 2 and cbo-pref > 50] != 0 [
;  create-turtles 1 [
;    set shape "face sad"
;    set label "Anti-Development CBO"
;    set label-color black
;    set stakeholder? 1
;    setxy -5 -50
;    set own-power sum [cbo-power] of cits with [turcbo = 2 and cbo-pref > 50]
;    set sown-power own-power
;    set own-pref (sum [cbo-eu] of cits with [turcbo = 2 and cbo-pref > 50])/(sum [cbo-power] of cits with [turcbo = 2 and cbo-pref > 50])
;    set sown-pref own-pref
;    set color 15
;    set size (count cits with [turcbo = 2 and cbo-pref > 50])/ 5
;   ]]
;  set citizenpref median [own-pref] of turtles with [stakeholder? = 1 and regulator? != 1]
  ask cits with [turcbo = 2] [stop]
  ]
  if ticks > 20 [
    ask cits with [label = "Pro-Development CBO"] [
      set stakeholder? 1
      set own-power sum [cbo-power] of cits with [turcbo = 2 and cbo-pref <= 50]
      set sown-power own-power
      set own-pref (sum [cbo-eu] of cits with [turcbo = 2 and cbo-pref <= 50])/(sum [cbo-power] of cits with [turcbo = 2 and cbo-pref <= 50] + 1)
      set sown-pref own-pref
  ;    set size (count cits with [turcbo = 2 and cbo-pref <= 50])/ 5
      set size 3
      set hidden? false
      set shape "face neutral"
      setxy -70 -50]
    ask cits with [label = "Anti-Development CBO"] [
      set stakeholder? 1
      set own-power sum [cbo-power] of cits with [turcbo = 2 and cbo-pref > 50]
      set sown-power own-power
      set own-pref (sum [cbo-eu] of cits with [turcbo = 2 and cbo-pref > 50])/(sum [cbo-power] of cits with [turcbo = 2 and cbo-pref > 50])
      set sown-pref own-pref
   ;   set size (count cits with [turcbo = 2 and cbo-pref > 50])/ 5
      set size 3
      set hidden? false
      set shape "face neutral"
      setxy -5 -50]
    set citizenpref citizenpref
    ask cits with [regulator? != 1] [stop]]
end

to regulator-vote
  if ticks = 25 [
  ifelse count (regulators with [rown-pref <= 50]) > count (regulators with [rown-pref > 50])
  [set regulators-anti false][set regulators-anti true]]
end

;This is the process by which community based organizations can arise.  Patches look around and see how much opposition there is.
;If there is a lot of opposition, the patch creates a community organization, which can then try to influence the patches
;nearby.  It's based on the average density of agents in the current model and the average square of opposition.

to patch-cbo
  ask patches [
    set cbonumb count cits with [turcbo = 2]
    ifelse any? cits with [turcbo = 2]
    [set avecbopref (sum [cbo-pref] of cits with [turcbo = 2]/ count cits with [turcbo = 2])]
    [set avecbopref 0]
    ifelse any? cits with [turcbo = 2] [
          set cbo? 1][set cbo? 0]
  ;  if (avecbopref > 0) and (avecbopref <= 20) and (pcolor != black) [set pcolor 105]
  ;  if (avecbopref > 20) and (avecbopref <= 40) and (pcolor != black) [set pcolor 115]
  ;  if (avecbopref > 40) and (avecbopref <= 60) and (pcolor != black) [set pcolor 125]
  ;  if (avecbopref > 60) and (avecbopref <= 80) and (pcolor != black) [set pcolor 135]
  ;  if (avecbopref > 80) and (avecbopref <= 100) and (pcolor != black) [set pcolor 15]
  ]
end



;This is the influence model.  I have set it up various ways to allow for different specifications.
;In the first way, attitude is pent up, and then released.
;In the second way, the agents convey their attitude every iteration.
;In the third way, agents convey their attitude every iteration, but only after reaching a certain threshold.

to influence
  if Influence-Model = 1 [
    ask cits [
      let firstpent pent
      set pent (firstpent + im)
      if pent >= 1 [
        set message (message + 1)
        set pent 0]]]
  if Influence-Model = 2 [
    ask cits [
      set message (message + im) ]]
  if Influence-Model = 3 [
    ask cits [
      if im >= Influence-Threshold [
        set message (message + im)]]]
  if Influence-Model = 4 [
    ask cits [
      if (abs im) >= Influence-Threshold [
        set message (message + im)]]]
end

;This determines whether the project succeeds or fails.  The model always runs to 100 interations.
;If method 1 above is used, it fails is the number of messages per agent is greater than 8.
;If the second or third model, it fails if the total message is greater than the number of agents.

  to-report ttpreference
    ifelse regulate? [
    if ticks = 23
    [report sum [tpreference] of cits]]
    [if ticks = 18
    [report sum [tpreference] of cits]]
  end

to continue
  ifelse regulate? [
    if ticks = 25 [
  ifelse regulators-anti [fail][success]]]
  [if ticks = 20 [stop]]
end

to litigation
  stop
  show "Litigation"
  ask patches [ set pcolor blue]
end

to success
;  if ticks >= 20 and quick? = 1 [
;    if Influence-Model = 1 [
;      ifelse sum [im] of cits >= 21 [fail set over? 1] [
;        ifelse sum [im] of cits <= 2.6 [succeed set over? 1] [set quick? 0
;          show quick?]]]
;    if Influence-Model = 2 [
;      ifelse sum [im] of cits >= 21 [fail set over? 1] [
;        ifelse sum [im] of cits <= 2.6 [succeed set over? 1] [set quick? 0
;          show quick?]]]
;    if Influence-Model = 3 [
;     ifelse sum [im] of cits >= 21 [fail set over? 1] [
;        ifelse sum [im] of cits <= 2.6 [succeed set over? 1] [set quick? 0
;          show quick?]]]
;    if Influence-Model = 4 [
;     ifelse sum [im] of cits >= 21 [fail set over? 1] [
;        ifelse sum [im] of cits <= 2.6 [succeed set over? 1] [set quick? 0
;          show quick?]]]]
;  if ticks >= 20 [
;    if Influence-Model = 1 [
;      ifelse sum [im] of cits >= 21 [fail][
;      ifelse sum [im] of cits <= 2.6[succeed][lawsuit]]]
;    if Influence-Model = 2 [
;     ifelse sum [im] of cits >= 21 [fail][
;      ifelse sum [im] of cits <= 2.6[succeed][lawsuit]]]
;    if Influence-Model = 3 [
;      ifelse sum [im] of cits >= 21 [fail][
;      ifelse sum [im] of cits <= 2.6[succeed][lawsuit]]]
;    if Influence-Model = 4 [
;      ifelse sum [im] of cits >= 21 [fail][
;      ifelse sum [im] of cits <= 2.6[succeed][lawsuit]]]
;    set over? 1]
   ifelse citizenpref <= 50 [succeed]
   [lawsuit]
end


;These procedures govern the response when the project either fails or succeed.  A message is printed and the patches change color.

to fail
  show "FAILURE!"
  ask patches
  [if pcolor != red [set pcolor black]]
  ask turtles [die]
  create-cits 1 [
    set shape "fire"
    setxy -40 -60
    set color red
    set size 50]
end

to succeed
  show "Welcome to a green energy future."
  ask patches
  [if pcolor != red [set pcolor green]]
  ask turtles [die]
  create-cits 1 [
    set shape "arrow 3"
    set heading 1
    setxy -40 -60
    set color gray
    set size 50]
end

to lawsuit
  show "Lawsuit"
  ask patches
  [if pcolor != red [set pcolor blue]]
  ask turtles [die]
  create-cits 1 [
    set shape "arrow 3"
    set heading 1
    setxy -40 -60
    set color gray
    set size 50]
  create-cits 1 [
    setxy -20 -75
    set shape "person"
    set color 24
    set size 10]
  create-cits 1 [
    setxy -65 -75
    set shape "person"
    set color 24
    set size 10]
  create-cits 1 [
    setxy -55 -85
    set shape "person"
    set color 24
    set size 10]
  create-cits 1 [
    setxy -35 -85
    set shape "person"
    set color 24
    set size 10]
end

;This is an output procedure to save output.  It creates a csv file for every model run, but is not very useful.

to do-output
  let output ticks + 1
  export-world (word "t" output ".csv")
end

;This is a procedure, executed at the end of each of the communication procedures above, that makes sure values are
;between 0 and 100.

to cap
    if idatt > 100    [set idatt 100]
    if idatt < 0      [set idatt 0]
end

;This procedure updates the turtle characteristics and global variables based on changes from the iteration.

to label-up
  ask cits [
    ;calculate the preference based on the ideological attitude and proximity
    set pref (((proximity * 100)  +  idatt )/ 2)
    ifelse cbo-pref != 0
    [set turcbo 2
      set shape "face neutral"
      if (cbo-pref > 0) and (cbo-pref <= 10) [
        set color 105
        set cboprefrange1 1]
      if (cbo-pref > 10) and (cbo-pref <= 20) [
        set color 105
        set cboprefrange2 1]
      if (cbo-pref > 20) and (cbo-pref <= 30) [
        set color 115
        set cboprefrange3 1]
      if (cbo-pref > 30) and (cbo-pref <= 40) [
        set color 115
        set cboprefrange4 1]
      if (cbo-pref > 40) and (cbo-pref <= 50) [
        set color 125
        set cboprefrange5 1]
      if (cbo-pref > 50) and (cbo-pref <= 60) [
        set color 125
        set cboprefrange6 1]
      if (cbo-pref > 60) and (cbo-pref <= 70) [
        set color 135
        set cboprefrange7 1]
      if (cbo-pref > 70) and (cbo-pref <= 80) [
        set color 135
        set cboprefrange8 1]
      if (cbo-pref > 80) and (cbo-pref <= 90) [
        set color 15
        set cboprefrange9 1]
      if (cbo-pref > 80) and (cbo-pref <= 90) [
        set color 15
        set cboprefrange10 1]
      ]

    [set turcbo 1
      set shape "person"]

    if (count my-out-links) + (count my-in-links) = 0 [
      set turcbo 1
      set cbo-pref 0
      set cbo-power 0
      set shape "person"]
    if (own-pref > 0) and (own-pref <= 10) [set prefrange1 1]
    if (own-pref > 10) and (own-pref <= 20) [set prefrange2 1]
    if (own-pref > 20) and (own-pref <= 30) [set prefrange3 1]
    if (own-pref > 30) and (own-pref <= 40) [set prefrange4 1]
    if (own-pref > 40) and (own-pref <= 50) [set prefrange5 1]
    if (own-pref > 50) and (own-pref <= 60) [set prefrange6 1]
    if (own-pref > 60) and (own-pref <= 70) [set prefrange7 1]
    if (own-pref > 70) and (own-pref <= 80) [set prefrange8 1]
    if (own-pref > 80) and (own-pref <= 90) [set prefrange9 1]
    if (own-pref > 90) and (own-pref <= 100) [set prefrange10 1]
    ;calculate salience based on the disruption of the project, the agent's proximity to it and the presence of a CBO
    set salience ( disruption * proximity * turcbo )
    ;calculate the salient preference as a benchmark
    set tpreference ( pref * salience )
    ;calculate the influence message based on preference, power, salience and a random term.
    set im (pref * power * salience * 0.9 + ran * 0.1) * 1.2 / (200 * 1.5)
    ;Determine the color and size in the visual display based on attitude and influence message.
    let redcolor (own-pref * 255 / 100)
    if redcolor < 0 [ set redcolor 0]
    if redcolor > 225 [set redcolor 225]
    let bluecolor ((100 - own-pref) * 255 / 100)
    if bluecolor < 0 [ set bluecolor 0]
    if bluecolor > 225 [set bluecolor 225]
    let intensity (abs (im) * 5 * disruption)
    ;let intensity (salience * 10 * abs (im))
    set color (list redcolor 0 bluecolor)
    ;if turcbo != 2 [set size abs(im) * 2.5]
    ;if turcbo = 2 [set size abs(im) * 5]
    set size intensity
  ]
  ask turtles with [stakeholder? = 1] [
      if (sown-pref > 0) and (sown-pref <= 10) [
        set color 105
        set scboprefrange1 1]
      if (sown-pref > 10) and (sown-pref <= 20) [
        set color 105
        set scboprefrange2 1]
      if (sown-pref > 20) and (sown-pref <= 30) [
        set color 115
        set scboprefrange3 1]
      if (sown-pref > 30) and (sown-pref <= 40) [
        set color 115
        set scboprefrange4 1]
      if (sown-pref > 40) and (sown-pref <= 50) [
        set color 125
        set scboprefrange5 1]
      if (sown-pref > 50) and (sown-pref <= 60) [
        set color 125
        set scboprefrange6 1]
      if (sown-pref > 60) and (sown-pref <= 70) [
        set color 135
        set scboprefrange7 1]
      if (sown-pref > 70) and (sown-pref <= 80) [
        set color 135
        set scboprefrange8 1]
      if (sown-pref > 80) and (sown-pref <= 90) [
        set color 135
        set scboprefrange9 1]
      if (sown-pref > 90) and (sown-pref <= 100) [
        set color 15
        set scboprefrange10 1]
    ]
  ask turtles with [regulator? = 1] [
      if (rown-pref > 0) and (rown-pref <= 10) [
        set color 105
        set rcboprefrange1 1]
      if (rown-pref > 10) and (rown-pref <= 20) [
        set color 105
        set rcboprefrange2 1]
      if (rown-pref > 20) and (rown-pref <= 30) [
        set color 115
        set rcboprefrange3 1]
      if (rown-pref > 30) and (rown-pref <= 40) [
        set color 115
        set rcboprefrange4 1]
      if (rown-pref > 40) and (rown-pref <= 50) [
        set color 125
        set rcboprefrange5 1]
      if (rown-pref > 50) and (rown-pref <= 60) [
        set color 125
        set rcboprefrange6 1]
      if (rown-pref > 60) and (rown-pref <= 70) [
        set color 135
        set rcboprefrange7 1]
      if (rown-pref > 70) and (rown-pref <= 80) [
        set color 135
        set rcboprefrange8 1]
      if (rown-pref > 80) and (rown-pref <= 90) [
        set color 135
        set rcboprefrange9 1]
      if (rown-pref > 90) and (rown-pref <= 100) [
        set color 15
        set rcboprefrange10 1]
    ]
  ;Embue the link with the influence message of the cit.
  ;Note: end1 is the end where the link is from, in this case the cit, not the projo.
  ask clopros [
    set citim [im] of end1]
  ask clopros [
    set citim [im] of end1]
  ask projos [
    set projoim sum [citim] of my-in-links
    let projcol max list (min list (projoim * 500) (255)) 0
    set color (list projcol 0 0)]
  set number-angry count cits with [pref > 80]
 ; set pref-std standard-deviation [pref] of cits
  set stakeinf (stakeinf + stakeim)
end

to report-pref
  set prefvariance variance [own-pref] of cits
  ;report mean [own-eu] of cits
end

to report-cbopref
 ; if ticks >= 1 [
  set cboprefvariance variance [cbo-pref] of cits with [turcbo = 2]
 ; ]
  ;report (count cits with [turcbo = 2])
end

;This procedure plots those changes over time.

to create-plots
  set-current-plot "Total Salient Preference"
  set-current-plot-pen "salience"
  plot (sum [tpreference] of cits)
  set-current-plot "Citizen Preference"
  set-plot-x-range 0 100
  set-plot-y-range 0 (count cits)
 ; set-plot-y-range -0 (precision (sum [own-power] of cits with [stakeholder? = 0]) 2)
 ; set-plot-y-range -0 (sum [own-power] of cits)
  set-histogram-num-bars 10
  histogram [own-pref] of cits
  set-current-plot "Stakeholder Preference"
  set-plot-x-range 0 100
  set-plot-y-range 0 (count turtles with [stakeholder? = 1])
 ; set-plot-y-range -0 (precision (sum [sown-power] of turtles with [stakeholder? = 1]) 2)
 ; set-plot-y-range -0 (sum [sown-power] of turtles with [stakeholder? = 1])
  set-histogram-num-bars 10
  set-plot-pen-mode 1
  histogram [sown-pref] of turtles with [stakeholder? = 1]
  if ticks > 15 [
  set-current-plot "Regulator Preference"
  set-plot-x-range 0 100
  set-plot-y-range -0 5
  set-histogram-num-bars 10
  set-plot-pen-mode 1
  histogram [rown-pref] of turtles with [regulator? = 1]]
  set-current-plot "Total Message"
  set-current-plot-pen "message"
  plot (sum [message] of cits)
end

to print-stakes
  ask cits [
    file-open "linkOut.csv"
file-print (word "stakes," xcor "," stakeholder?)
  ]
end