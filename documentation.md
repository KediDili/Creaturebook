# Creating content packs for Creaturebook
Creaturebook is a framework that allows other modders to add their animals or plants to the mod's own book. 

**Warning! After this point, this documentation will assume that you have experience with creating content packs. If not, please see this wiki page**
**https://stardewvalleywiki.com/Modding:Content_packs#For_modders**

# File structure
All content packs require a ``chapter.json`` to have along with the ``manifest.json``. Near these, it is also required a subfolder that's named after the ``chapter.json``'s ``ChapterTitle`` string field. Under that subfolder, there should be finally subfolders that contain a ``creature.json`` and a ``book-image.png`` for each creature.
So it would look like this:
```
ModName > ChapterTitle > AnySubfolderName > creature.json
          manifest.json                     book-image.png
          chapter.json
```

If you would like this approach, let's make an example out of Project Starlight below:
```
[CB] Project Starlight > Moths > Anomalous Bluetail > creature.json
                         manifest.json                book-image.png
                         chapter.json
```

# Formatting for chapter.json
Each ``chapter.json`` is possible to have these fields below:

``"CreatureAmount"``
Value type: ``int``
Is it required: Yes
Notes: This has to match with how many creature subfolders you have in your chapter. If it's not, Creaturebook will show a warning and your 
content pack will be ignored.
Example: ``"CreatureAmount": 55``

``"ChapterTitle"``
Value type: ``string``
Is it required: Yes
Notes: Your subfolder near the ``chapter.json`` and ``manifest.json`` need to be named after this field. Also will be displayed on your chapter's header page in the book's own menu.
Example: ``"ChapterTitle": "Moths"``

``"CreatureNamePrefix"``
Value type: ``string``
Is it required: Yes
Notes: This has to be unique among all Creaturebook content packs, since the mod does its data storing using this, ``CreatureID`` and the packs's unique ID, it's not recommended to use your author name or the mod ID to differentiate it, as it could cause bugs or errors.
Example: ``"CreatureNamePrefix": "Flutter"``

``"Author"``
Value type: ``string``
Is it required: No
Notes: This will be displayed on your chapter's header page in the mod's book menu, it wont cause errors if omitted, but will default to a ridiculous string to display.
Example: ``"Author": "Kedi"``

# Formatting for creature.json
Each ``creature.json`` is possible to have these fields below:

``"CreatureID": 32``
Value type: ``string``
Is it required: No
Notes: This will be displayed on your chapter's header page in the mod's book menu, it wont cause errors if omitted, but will default to a ridiculous string to display.
