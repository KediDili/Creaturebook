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

- ``"CreatureAmount"``

Value type: ``int``
Is it required: Yes
Notes: This has to match with how many creature subfolders you have in your chapter. If it's not, Creaturebook will show a warning and your 
content pack will be ignored.
Example: ``"CreatureAmount": 55``

- ``"ChapterTitle"``

Value type: ``string``
Is it required: Yes

Notes: Your subfolder near the ``chapter.json`` and ``manifest.json`` need to be named after this field. Also will be displayed on your chapter's header page in the book's own menu.

Example: ``"ChapterTitle": "Moths"``

- ``"CreatureNamePrefix"``

Value type: ``string``
Is it required: Yes
Notes: This has to be unique among all Creaturebook content packs, since the mod does its data storing using this, ``CreatureID``(see below) and the packs's unique ID, it's not recommended to use your author name or the mod ID to differentiate it, as it could cause bugs or errors.
Example: ``"CreatureNamePrefix": "Flutter"``

- ``"Author"``

Value type: ``string``
Is it required: No
Notes: This will be displayed on your chapter's header page in the mod's book menu, it wont cause errors if omitted, but will default to a ridiculous string to display.
Example: ``"Author": "Kedi"``

# Formatting for creature.json
Each ``creature.json`` is possible to have these fields below:

- ``"CreatureID"``

Value type: ``int``
Is it required: Yes. Absolutely
Notes: This is where your creature is in the chapter (This means the smaller ID a creature has, the earlier its page will be). It has to be unique for each creature that are in the same chapter. It always should begin with the first one having ``0`` as its ID, next one ``1``, then ``2``, and so goes on.
Example: ``"CreatureID": 32``

- ``"HasExtraImages"``

Value type: ``bool``
Is it required: Yes
Notes: This sets if your creature owns one or two extra images to display on the Creaturebook menu. If enabled, it'll require the said creature to have a ``book-image_2.png`` under its subfolder. You can also have a ``book-image_3.png``, but it's optional even with this option enabled.
Example: ``"HasExtraImages": false``

- ``"HasFunFact"``
 
Value type: ``bool``
Is it required: Yes
Notes: This sets if your creature has a fact to be shared when its discovered on Creaturebook menu's right side. If enabled, would require you to add an ``i18n`` folder (See translations), inside the translation files, your fact should be stored like``"<CreatureNameprefix>_<CreatureID>" : "My creature fact"``
Example: ``"HasFunFact": true``

- ``"HasScientificName"`` 

Value type: ``bool``
Is it required: Yes
Notes: This sets if your creature should have a scientific name.
Example: ``"HasScientificName": true``

- ``"ScientificName"``

Value type: ``string``
Is it required: Only if ``HasScientificName`` is enabled.
Notes: This is your creature's Latin scientific name. It will be displayed on the mod menu once your creature is discovered.
Example: ``"ScientificName": "Ormetica ameoides"``

- ``"OffsetX"``

Value type: ``int``
Is it required: No
Notes: This offsets your creature's ``book-image.png``'s placement on X axis.
Example: ``"OffsetX": 0``

- ``"OffsetY"``

Value type: ``int``
Is it required: No
Notes: Does the same thing with ``OffSetX``, but for Y axis.
Example: ``"OffsetY": 0``

- ``ImageScale_1``

Value type: ``float``
Is it required: No
Notes: This changes the scale of your creature's ``book-image.png``. The bigger the value is, the bigger it'll look!
Example: ``"ImageScale_1": 7``

- ``"OffsetX_2"``

Value type: ``int``
Is it required: No
Notes:  Does the same thing with ``OffSetX``, but for ``book-image_2.png``.
Example: ``"OffsetX_2": -30``

- ``"OffsetY_2"``

Value type: ``int``
Is it required: No
Notes: Does the same thing with ``OffSetY``, but for ``book-image_2.png``.
Example: ``"OffsetY_2": 10``

- ``ImageScale_2``

Value type: ``float``
Is it required: Only if the creature has a ``book-image_2.png``
Notes:  Does the same thing with ``ImageScale_1``, but for ``book-image_2.png``.
Example: ``"ImageScale_2": 5``

- ``"OffsetX_3"``

Value type: ``int``
Is it required: No
Notes:  Does the same thing with ``OffSetX``, but for ``book-image_3.png``.
Example: ``"OffsetX_3": 0``

- ``"OffsetY_3"``

Value type: ``int``
Is it required: No
Notes: Does the same thing with ``OffSetY``, but for ``book-image_2.png``.
Example: ``"OffsetY_3": 15``

- ``ImageScale_3``

Value type: ``float``
Is it required: Only if the creature has a ``book-image_3.png``
Notes:  Does the same thing with ``ImageScale_1``, but for ``book-image_3.png``.
Example: ``"ImageScale_3": 4``

-``OverrideDefaultNaming``

Value type: ``string``
Is it required: No
Notes: This is used if you want your creature to be found by clicking an NPC-in-game-code that has a different internal name than how does Creaturebook names creatures internally.
Example: ``"OverrideDefaultNaming": "Mr.Qi"``
