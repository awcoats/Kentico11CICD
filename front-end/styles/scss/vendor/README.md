# Vendors

Most projects will have a `vendors/` folder containing all the CSS files from external libraries and frameworks – Normalize, Bootstrap, jQueryUI, FancyCarouselSliderjQueryPowered, and so on. Putting those aside in the same folder is a good way to say “Hey, this is not from me, not my code, not my responsibility”.

If you have to override a section of any vendor, I recommend you have an 8th folder called `vendors-extensions/` in which you may have files named exactly after the vendors they overwrite. For instance, `vendors-extensions/_bootstrap.scss` is a file containing all CSS rules intended to re-declare some of Bootstrap’s default CSS. This is to avoid editing the vendor files themselves, which is generally not a good idea.

Reference: [Sass Guidelines](http://sass-guidelin.es/) > [Architecture](http://sass-guidelin.es/#architecture) > [Vendors folder](http://sass-guidelin.es/#vendors-folder)

!iIMPORTANT!
Bootstrap instructions
1. Make sure gulp is not running
2. Download source files for bootstrap (I recommend cloning their github repo locally and pulling from there)
3. from the bootstrap source files, copy everything from the "bootstrap/scss" folder into the "bootstrap" directory here
4. Delete bootstrap-grid.scss and bootstrap-reboot from the "bootstrap" folder
5. Place bootstrap.scss in this folder
6. Rename to _bootstrap.scss
7. Add the bootstrap folder to all paths in _bootstrap.scss. e.g.:
	@import "grid";
	becomes
	@import "bootstrap/grid";
8. Open style/scss/main.scss and uncomment this chunk of code:
	@import
	  'vendor/bootstrap';

Now bootstrap is properly included in the structure and should compile into main when you run the gulp
command on the main folder.

Not all bootstrap partials may be neccessary for the project you are working on so just comment out any unnecessary partials in _bootstrap.scss
