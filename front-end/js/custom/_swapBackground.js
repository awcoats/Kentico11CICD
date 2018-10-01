$(document).ready(function() {
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

(function($) {
    $.fn.swapBackground = function(options) {
        var opts = options;
        return this.each(function() {
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
                        $this.css('background-image', 'url(' + srcSm + ')')
                    } else {
                        $this.css('background-image', '')
                    }
                } else {
                    if ($this.attr('data-lg')) {
                        $this.css('background-image', 'url(' + srcLg + ')')
                    } else {
                        $this.css('background-image', '')
                    }
                }
            }
            swapImages();
            $(window).resize(function() { swapImages() })
        })
    }
}(jQuery))