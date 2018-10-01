# vendor

This is where un-modified scripts originating from any third party should be placed.
This includes plugins, libraries and scripts belonging to frameworks such as bootstrap.

1. Place un-modified individual vendor scripts in this directory
2. Scripts within this directory shuld be candidates for updating so if they require calls, etc. please place those within a custom script in the "custom" directory
3. These scripts will be compiled into frontEnd.js in no particular order and do not need to be included in the imports file