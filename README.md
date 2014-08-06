Renderer for Just Cause 2 model (RBM) assets
==============

Currently a **```WORK IN PROGRESS```**.

Usage
==============
Open any small archive (.ee, .eez, etc.) using the File->Open menu. Most models will appear to be incorrectly textured until you set the ```UnpackedGeneral``` path to an extracted ```general.blz``` via the ```RBMRender.exe.config``` file in the compiled output directory.

For now, you will need to extract general.blz. Eventually, the tool will automatically locate and load this file.

General.blz path
===============
An example of what a path may look like:

```
<setting name="UnpackedGeneral" serializeAs="String">
    <value>C:\Path\To\Unpacked\General.blz\</value>
</setting>
```
