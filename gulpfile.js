'use strict';

// --------------------------------------------------------------->>>
// Task Configuration Object
// --------------------------------------------------------------->>>

// get gulp parameter
const argv = require('yargs').argv

// project directory
const FEDDEV = './front-end';
const BEDDEV = './server/CMS';

// project configuration
const CONFIG = {
	IMG: {
		input: FEDDEV + '/img/**/**',
		output: argv.prod ? BEDDEV + '/img/' : FEDDEV + '/img/',
		watcher: FEDDEV + '/img/**/**'
	},
	FONTS: {
		input: FEDDEV + '/fonts/**/**',
		output: argv.prod ? BEDDEV + '/fonts/' : FEDDEV + '/fonts/',
		watcher: FEDDEV + '/fonts/**/**'
	},
	JS: {
		input: FEDDEV + '/js/',
		output: argv.prod ? BEDDEV + '/dist/js/' : FEDDEV + '/js/',
		watcher: [FEDDEV + '/js/custom/*.js', FEDDEV + '/js/vendor/*.js'],
		babel: {
			sourceType: "script", 
			presets: [
				[
					"env", 
					{
						"targets": {
							"browsers": ["last 2 versions", "ie >= 10"]
						}
					}
				]
			]
		},
		concat: {
			// js:compile|js:minify will use the pattern to combine custom and vendor compilations to one file
			all: 'frontEnd.js',
			// js:vendor:compile will concatenate vendor js to this file
			vendor: 'vendor.js',
			// js:custom:compile will use this file to create a concatenation of all custom includes
			custom: 'custom.js',
		},
		// js:custom:compile import source
		custom() {
			return CONFIG.JS.input + 'custom/custom.js';
		},
		// js:vendor:compile will concat your vendor files declare the patterns and order here
		vendor() {
			return [
				CONFIG.JS.input + 'vendor/jquery*',
				CONFIG.JS.input + 'vendor/popper*',
				CONFIG.JS.input + 'vendor/tether*',
				CONFIG.JS.input + 'vendor/bootstrap*',
				CONFIG.JS.input + 'vendor/**/*.js',
			];
		},
		// minification options
		minify() {
			return {
				// noSource: true  //uncomment this line to prevent the creation of a non-minified frontEnd.js
			};
		},
		// custom include options
		include() {
			return {
				extensions: 'js',
				hardFail: false,
			};
		},
	},
	PUG: {
		input: FEDDEV + '/pug/*.pug',
		output: FEDDEV + '/',
		watcher: FEDDEV + '/pug/**/*.pug',
		// pug compile extension
		extension: '.html',
		// pug compile options
		options() {
			return {
				pretty: true,
				notify: false,
				verbose: true,
				data: {  },
				filters: {
					escape: (text, params) => {
						let map = {
							'&': '&amp;',
							'<': '&lt;',
							'>': '&gt;',
							'"': '&quot;',
							"'": '&#039;'
						};
						let esc = pug.render(text);
						return esc.replace(/[&<>"']/g, (m) => {
							return map[m];
						});
					},
				},
			};
		},
	},
	SASS: {
		input: FEDDEV + '/styles/scss/main.scss',
		output: argv.prod ? BEDDEV + '/dist/styles/' : FEDDEV + '/styles/',
		watcher: FEDDEV + '/styles/scss/**/*.s*ss',
		// node-sass options
		options() {
			return {
				outputStyle: 'compressed',
				precision: 5,
			};
		},
		// autoprefix options
		prefixer() {
			return {
				browsers: ['last 2 versions'],
				cascade: false,
				grid: true
			};
		},
		// sourcemaps options
		sourcemaps() {
			return './'; // this will create a sourcemap where the css is compiled
		},
	},
	PLUMBER: {
		// plumber options
		options() {
			return {
				errorHandler: notify.onError("<%= error.message %>"),
			};
		},
	},
	CLEANDEV: {
		// clean:dev paths and patterns
		paths() {
			return [
				CONFIG.JS.output + '*.js',
				CONFIG.PUG.output + '*.html',
				CONFIG.SASS.output + '*.css',
				CONFIG.SASS.output + '*.map',
				CONFIG.SITECORE.manifest,
			].concat(
				argv.prod
					?
					[
						CONFIG.IMG.output + '**',
						CONFIG.FONTS.output + '**'
					]
					:
					[]
			);
		}
	},
	SITECORE: {
		// sitecore:update url and Authorization key
		appUrl: 'http://amgen-dev-2016.bluemod.us/api-updatecontent/index',
		appKey: '6468260c-f8a3-4679-a540-2d7fbfcb7cf4',
		// sitecore content manifest for AJAX posts
		manifest: './content.json',
	},
	BROWSERSYNC: {
		// browser:sync options
		options() {
			return {
				// serve files from
				server: {
					baseDir: FEDDEV,
					serveStaticOptions: {
						extensions: ['html']
					}
				},
				notify: false,
				reloadDebounce: 100
			};
		},
	},
};

// --------------------------------------------------------------->>>
// Task Dependencies
// --------------------------------------------------------------->>>
const autoprefix = require('gulp-autoprefixer'),
	babel = require('gulp-babel'),
	browsersync = require('browser-sync').create(),
	chalk = require('chalk'),
	changed = require('gulp-changed'),
	concat = require('gulp-concat'),
	del = require('del'),
	ext = require('gulp-util').replaceExtension,
	gulp = require('gulp-help')(require('gulp')),
	include = require('gulp-include'),
	log = require('gulp-util').log,
	minify = require('gulp-minify'),
	notify = require('gulp-notify'),
	pluginerror = require('gulp-util').PluginError,
	plumber = require('gulp-plumber'),
	pug = require('pug'),
	run = require('run-sequence'),
	sass = require('gulp-sass'),
	sourcemaps = require('gulp-sourcemaps'),
	through = require('through2'),
	watch = require('gulp-watch');

// --------------------------------------------------------------->>>
// Gulp Tasks
// --------------------------------------------------------------->>>

/****** FONT:COPY ******/
gulp.task('font:copy', 'Copy font assets to output folder', () => {
	return gulp.src(CONFIG.FONTS.input)
		.pipe(gulp.dest(CONFIG.FONTS.output));
});

/****** IMG:COPY ******/
gulp.task('img:copy', 'Copy image assets to output folder', () => {
	return gulp.src(CONFIG.IMG.input)
		.pipe(gulp.dest(CONFIG.IMG.output));
});

/****** JS:STREAM ******/
gulp.task('js:stream', ['js:compile'], () => {
	return gulp.src(CONFIG.JS.output + '*.js')
		.pipe(browsersync.stream());
});


/****** JS:CUSTOM:COMPILE ******/
gulp.task('js:custom:compile', 'Compile custom js with includes to custom.js', () => {
	return gulp.src(CONFIG.JS.custom())
		.pipe(plumber(CONFIG.PLUMBER.options()))
		.pipe(include(CONFIG.JS.include()))
		.pipe(babel(CONFIG.JS.babel))
		.pipe(plumber.stop())
		.pipe(gulp.dest(CONFIG.JS.output));
});

/****** JS:VENDOR:COMPILE ******/
gulp.task('js:vendor:compile', 'Compile vendor js to vendor.js', () => {
	return gulp.src(CONFIG.JS.vendor())
		.pipe(plumber(CONFIG.PLUMBER.options()))
		.pipe(concat(CONFIG.JS.concat.vendor))
		.pipe(babel(CONFIG.JS.babel))
		.pipe(plumber.stop())
		.pipe(gulp.dest(CONFIG.JS.output));
});

/****** JS:COMPILE ******/
gulp.task('js:compile', 'Concatenate vendor.js and custom.js to frontEnd.js && frontEnd.min.js', ['js:custom:compile', 'js:vendor:compile'], () => {
	return gulp.src([
		CONFIG.JS.output + CONFIG.JS.concat.vendor,
		CONFIG.JS.output + CONFIG.JS.concat.custom,
	])
		.pipe(plumber(CONFIG.PLUMBER.options()))
		.pipe(concat(CONFIG.JS.concat.all))
		.pipe(minify(CONFIG.JS.minify()))
		.pipe(plumber.stop())
		.pipe(gulp.dest(CONFIG.JS.output));
});

/****** JS:STREAM ******/
gulp.task('js:stream', ['js:compile'], () => {
	return gulp.src(CONFIG.JS.output + '*.js')
		.pipe(browsersync.stream());
});

/****** CSS:COMPILE ******/
gulp.task('css:compile', 'Compile main SASS file to main.css and create sourcemap main.css.map', () => {
	return gulp.src(CONFIG.SASS.input)
		.pipe(plumber(CONFIG.PLUMBER.options()))
		.pipe(sourcemaps.init())
		.pipe(sass(CONFIG.SASS.options()))
		.pipe(autoprefix(CONFIG.SASS.prefixer()))
		.pipe(sourcemaps.write(CONFIG.SASS.sourcemaps()))
		.pipe(plumber.stop())
		.pipe(gulp.dest(CONFIG.SASS.output));
});

/****** CSS:STREAM ******/
gulp.task('css:stream', ['css:compile'], () => {
	return gulp.src(CONFIG.SASS.output + '*.css')
		.pipe(browsersync.stream());
});

/****** PUG:COMPILE ******/
gulp.task('pug:compile', 'Compile PUG files to HTML', () => {
	return gulp.src(CONFIG.PUG.input)
		.pipe(plumber(CONFIG.PLUMBER.options()))
		.pipe(changed(CONFIG.PUG.output, { extension: CONFIG.PUG.extension, hasChanged: changed.compareContents }))
		.pipe(through.obj((file, enc, cb) => {
			// get pug options
			let options = CONFIG.PUG.options();
			let data = Object.assign({}, options.data, file.data || {});
			// store pug file path
			options.filename = file.path;
			// change file extension to .html
			file.path = ext(file.path, CONFIG.PUG.extension);
			// compile the file contents
			if (file.isBuffer()) {
				try {
					if (options.verbose === true) {
						log(chalk.green.bold('Compiling File:') + ' ' + options.filename);
					}
					let contents = String(file.contents); // convert buffer to string
					let compiled = pug.compile(contents, options)(data);
					file.contents = new Buffer(compiled);
				} catch (e) {
					return cb(new pluginerror('pug:compile', e));
				}
				return cb(null, file);
			}
		}))
		.pipe(plumber.stop())
		.pipe(gulp.dest(CONFIG.PUG.output));
});

/****** PUG:STREAM ******/
gulp.task('pug:stream', ['pug:compile'], () => {
	return gulp.src(CONFIG.PUG.output + '*.html')
		.pipe(browsersync.stream());
});

/****** CLEAN:DEV ******/
gulp.task('clean:dev', 'Delete all compiled files', () => {
	return del(CONFIG.CLEANDEV.paths())
		.then(paths => {
			paths.forEach((f) => {
				log(chalk.red.bold('Deleting File:') + ' ' + f);
			});
		});
});

/****** WATCH:FILES ******/
gulp.task('watch:files', false, () => {
	watch(CONFIG.JS.watcher, function () { run('js:stream'); });
	watch(CONFIG.PUG.watcher, function () { run('pug:stream'); });
	watch(CONFIG.SASS.watcher, function () { run('css:stream'); });
});

/****** BROWSER:SYNC ******/
gulp.task('browser:sync', 'Launch local development with BrowserSync and watch for file changes', ['js:compile', 'css:compile', 'pug:compile', 'watch:files'], () => {
  del([CONFIG.JS.output + CONFIG.JS.concat.custom, CONFIG.JS.output + CONFIG.JS.concat.vendor])
    .then(paths => {
      paths.forEach((f) => {
        log(chalk.red.bold('Deleting File:') + ' ' + f);
      });
    });
  let options = CONFIG.BROWSERSYNC.options();
      // set unique ports
      options.port = Math.floor(Math.random() * (49150 - 1024 + 1)) + 1024;
      options.ui = { port: (options.port + 1) };
  return browsersync.init(options);
});

/****** CONTENT:EXTRACT ******/
gulp.task('sitecore:extract', '(SiteCore) Create extraction manifest from html files in content.json', ['pug:compile'], () => {
	// custom function file
	let sitecore = require('@blue-modus/gulp-sitecore');
	return sitecore.contentExtract(CONFIG.SITECORE.manifest, CONFIG.PUG.output + '*' + CONFIG.PUG.extension);
});

/****** UPDATE:SITECORE ******/
gulp.task('sitecore:update', '(SiteCore) Auto update items with ajax posting', () => {
	// custom function file
	let sitecore = require('@blue-modus/gulp-sitecore');
	return sitecore.updateSitecore(CONFIG.SITECORE.manifest, CONFIG.SITECORE.appUrl, CONFIG.SITECORE.appKey);
});
