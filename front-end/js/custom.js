'use strict';

/*
	This is where you include your JS files. You can change the
	order of inclusion by changing the order of the list in 
	this file.

	All script names should begin with an underscore.

	Imports should be formatted like this: //=require _myScript.js

	Here we are pulling in _retach.js - do the same thing for other files you want to include.

*/

$(document).ready(function () {

	// Move .something to .somewhere when the screen is smaller than 992, put it back when its larger.

	//   $('.something').retach({

	//     destination: '.somewhere',

	//     mediaQuery: 992

	//   });

});

(function ($) {

	$.fn.retach = function (opts) {

		var defaults = {

			destination: 'body',

			mediaQuery: 1023,

			movedClass: 'is-moved',

			moveMethod: 'append',

			wrapper: '',

			callback_move: function callback_move() {

				return;
			},

			callback_back: function callback_back() {

				return;
			}

		};

		var options = $.extend({}, defaults, opts);

		var $items = this;

		var $dest = $(options.destination);

		var mq = options.mediaQuery;

		var mc = options.movedClass;

		var mm = options.moveMethod;

		var cm = options.callback_move;

		var cb = options.callback_back;

		if (options.wrapper !== '') {

			var wrapperID = Math.floor(Math.random() * 10000 + 1) + Math.floor(Math.random() * 10000 + 1);

			var $wrap = $('<' + options.wrapper + '/>', { 'data-wrapperid': wrapperID });
		}

		var placeholderID = Math.floor(Math.random() * 10000 + 1) + Math.floor(Math.random() * 10000 + 1);

		var $placeholder = $('<i class="placeholder" data-placeholderid="' + placeholderID + '" />');

		$placeholder.css('display', 'none');

		$items.each(function (index) {

			if (index === 0) {

				$(this).before($placeholder);
			}
		});

		if (mm === 'prepend') {

			$($items.get().reverse()).each(function (index) {

				var $item = $(this);

				moveMe($item);

				$(window).on('load resize', function () {

					moveMe($item);
				});
			});

			$items.each(function (index) {

				var $item = $(this);

				putMeBack($item);

				$(window).on('load resize', function () {

					putMeBack($item);
				});
			});
		}

		if (mm === 'append') {

			$items.each(function (index) {

				var $item = $(this);

				moveMe($item);

				$(window).on('load resize', function () {

					moveMe($item);
				});
			});

			$($items.get().reverse()).each(function (index) {

				var $item = $(this);

				putMeBack($item);

				$(window).on('load resize', function () {

					putMeBack($item);
				});
			});
		}

		function moveMe($el) {

			var shouldMove = window.matchMedia("(max-width: " + mq + "px)").matches;

			var hasClass = $el.hasClass(mc);

			if (shouldMove && !hasClass) {

				if (mm === 'prepend') {

					$dest.prepend($el);
				} else if (mm === 'append') {

					$dest.append($el);
				}

				$el.addClass(mc);

				cm.call(this);

				if ($wrap != void 0) {

					$el.wrap($wrap);
				}
			}

			return;
		}

		function putMeBack($el) {

			var shouldMove = window.matchMedia("(max-width: " + mq + "px)").matches;

			var hasClass = $el.hasClass(mc);

			if (!shouldMove) {

				if ($wrap != void 0) {

					if ($el.parent().attr('data-wrapperid') == wrapperID) {

						$el.unwrap();
					}
				}

				$placeholder.after($el);

				$el.removeClass(mc);

				cb.call(this);
			}

			return;
		}
	};
})(jQuery);
$(document).ready(function () {
	/*
 * swap lg/sm background images on elements by classname.
 * .swapBackground included as a utility
 *USAGE:
 (pug)
 .heroMain(data-sm="/img/content-images/heroBg-sm.jpg", data-lg="/img/content-images/heroBg-lg.jpg")
   (this file)
 $('.heroMain').swapBackground({
         mediaQuery: 768
 });
 
 */
	$('.swapBackground').swapBackground({
		mediaQuery: 768
	});
});

(function ($) {
	$.fn.swapBackground = function (options) {
		var opts = options;
		return this.each(function () {
			var defaults = { srcSm: $(this).data("sm"), srcLg: $(this).data("lg"), mediaQuery: 1023 };
			var options = $.extend({}, defaults, opts);
			var $this = $(this);
			var srcSm = options.srcSm;
			var srcLg = options.srcLg;
			var mediaQuery = options.mediaQuery;

			function swapImages() {
				if (window.matchMedia("(max-width: " + mediaQuery + "px)").matches) {
					if ($this.attr('data-sm')) {
						console.log('got it');
						$this.css('background-image', 'url(' + srcSm + ')');
					} else {
						$this.css('background-image', '');
					}
				} else {
					if ($this.attr('data-lg')) {
						$this.css('background-image', 'url(' + srcLg + ')');
					} else {
						$this.css('background-image', '');
					}
				}
			}
			swapImages();
			$(window).resize(function () {
				swapImages();
			});
		});
	};
})(jQuery);
$(document).ready(function () {
	/* scripting for .headerMain*/
});