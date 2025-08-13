(function ($) {
    "use strict";
    // TOP Menu Sticky
    $(window).on('scroll', function () {
        var scroll = $(window).scrollTop();
        if (scroll < 400) {
            $("#sticky-header").removeClass("sticky");
            $('#back-top').fadeIn(500);
        } else {
            $("#sticky-header").addClass("sticky");
            $('#back-top').fadeIn(500);
        }
    });





    $(document).ready(function () {

        // mobile_menu
        var menu = $('ul#navigation');
        if (menu.length) {
            menu.slicknav({
                prependTo: ".mobile_menu",
                closedSymbol: '+',
                openedSymbol: '-'
            });
        };
        // blog-menu
        // $('ul#blog-menu').slicknav({
        //   prependTo: ".blog_menu"
        // });

        // review-active

        var slider = $('.slider_active');
        if (slider.length) {
            slider.owlCarousel({
                loop: true,
                margin: 0,
                items: 1,
                autoplay: true,
                navText: ['<i class="ti-angle-left"></i>', '<i class="ti-angle-right"></i>'],
                nav: true,
                dots: false,
                autoplayHoverPause: true,
                autoplaySpeed: 800,
                responsive: {
                    0: {
                        items: 1,
                        nav: false,
                    },
                    767: {
                        items: 1,
                        nav: false,
                    },
                    992: {
                        items: 1,
                        nav: false
                    },
                    1200: {
                        items: 1,
                        nav: false
                    },
                    1600: {
                        items: 1,
                        nav: true
                    }
                }
            });
        }



        // review-active
        var testmonial = $('.testmonial_active');
        if (testmonial.length) {
            testmonial.owlCarousel({
                loop: true,
                margin: 0,
                autoplay: true,
                navText: ['<i class="ti-angle-left"></i>', '<i class="ti-angle-right"></i>'],
                nav: true,
                dots: false,
                autoplayHoverPause: true,
                autoplaySpeed: 800,
                responsive: {
                    0: {
                        items: 1,
                        dots: false,
                        nav: false,
                    },
                    767: {
                        items: 1,
                        dots: false,
                        nav: false,
                    },
                    992: {
                        items: 1,
                        nav: true
                    },
                    1200: {
                        items: 1,
                        nav: true
                    },
                    1500: {
                        items: 1
                    }
                }
            });
        }

        // review-active
        var candidate = $('.candidate_active');
        if (candidate.length) {
            candidate.owlCarousel({
                loop: true,
                margin: 30,
                autoplay: true,
                navText: ['<i class="ti-angle-left"></i>', '<i class="ti-angle-right"></i>'],
                nav: true,
                dots: false,
                autoplayHoverPause: true,
                autoplaySpeed: 800,
                responsive: {
                    0: {
                        items: 1,
                        dots: false,
                        nav: false,
                    },
                    767: {
                        items: 3,
                        dots: false,
                        nav: false,
                    },
                    992: {
                        items: 4,
                        nav: true
                    },
                    1200: {
                        items: 4,
                        nav: true
                    },
                    1500: {
                        items: 4
                    }
                }
            });
        }




        // for filter
        // init Isotope
        var $grid = $('.grid').isotope({
            itemSelector: '.grid-item',
            percentPosition: true,
            masonry: {
                // use outer width of grid-sizer for columnWidth
                columnWidth: 1
            }
        });

        // filter items on button click
        $('.portfolio-menu').on('click', 'button', function () {
            var filterValue = $(this).attr('data-filter');
            $grid.isotope({ filter: filterValue });
        });

        //for menu active class
        $('.portfolio-menu button').on('click', function (event) {
            $(this).siblings('.active').removeClass('active');
            $(this).addClass('active');
            event.preventDefault();
        });

        // wow js
        new WOW().init();

        // counter 
        $('.counter').counterUp({
            delay: 10,
            time: 10000
        });

        ///* magnificPopup img view */
        //$('.popup-image').magnificPopup({
        //	type: 'image',
        //	gallery: {
        //	  enabled: true
        //	}
        //});

        ///* magnificPopup img view */
        //$('.img-pop-up').magnificPopup({
        //	type: 'image',
        //	gallery: {
        //	  enabled: true
        //	}
        //});

        ///* magnificPopup video view */
        //$('.popup-video').magnificPopup({
        //	type: 'iframe'
        //});

        // blog-page

        //brand-active
        var brand = $('.brad_active');
        if (brand.length) {
            brand.owlCarousel({
                loop: true,
                autoplay: true,
                nav: false,
                dots: false,
                autoplayHoverPause: true,
                autoplaySpeed: 800,
                responsive: {
                    0: {
                        items: 2,
                        nav: false
                    },
                    767: {
                        items: 4
                    },
                    992: {
                        items: 5
                    }
                }
            });
        }


        // blog-dtails-page

        if (document.getElementById('default-select')) {
            $('select').niceSelect();
        }

        //about-pro-active
        $('.details_active').owlCarousel({
            loop: true,
            margin: 0,
            items: 1,
            // autoplay:true,
            navText: ['<i class="ti-angle-left"></i>', '<i class="ti-angle-right"></i>'],
            nav: true,
            dots: false,
            // autoplayHoverPause: true,
            // autoplaySpeed: 800,
            responsive: {
                0: {
                    items: 1,
                    nav: false

                },
                767: {
                    items: 1,
                    nav: false
                },
                992: {
                    items: 1,
                    nav: false
                },
                1200: {
                    items: 1,
                }
            }
        });

    });

    // resitration_Form
    $(document).ready(function () {
        $('.popup-with-form').magnificPopup({
            type: 'inline',
            preloader: false,
            focus: '#name',

            // When elemened is focused, some mobile browsers in some cases zoom in
            // It looks not nice, so we disable it:
            callbacks: {
                beforeOpen: function () {
                    if ($(window).width() < 700) {
                        this.st.focus = false;
                    } else {
                        this.st.focus = '#name';
                    }
                }
            }
        });
    });



    //------- Mailchimp js --------//  
    function mailChimp() {
        $('#mc_embed_signup').find('form').ajaxChimp();
    }
    mailChimp();



    // Search Toggle
    $("#search_input_box").hide();
    $("#search").on("click", function () {
        $("#search_input_box").slideToggle();
        $("#search_input").focus();
    });
    $("#close_search").on("click", function () {
        $('#search_input_box').slideUp(500);
    });
    // Search Toggle
    $("#search_input_box").hide();
    $("#search_1").on("click", function () {
        $("#search_input_box").slideToggle();
        $("#search_input").focus();
    });
    $(document).ready(function () {
        $('select').niceSelect();
    });


    //   $('#datepicker').datepicker({
    //     iconsLibrary: 'fontawesome',
    //     icons: {
    //      rightIcon: '<span class="fa fa-caret-down"></span>'
    //  }
    // });


    $(document).ready(function() {
        console.log('AllJobs page initializing...');
        
        // Initialize nice-select with custom configuration
        if (typeof $.fn.niceSelect !== 'undefined') {
            $('select.nice-select').niceSelect();
            console.log('Nice-select initialized successfully');
        } else {
            console.warn('Nice-select plugin not found, using default selects');
        }
        
        // Handle filter form changes with debouncing
        $('#filterForm select').on('change', function() {
            var selectName = $(this).attr('name');
            console.log('Filter changed:', selectName, '=', $(this).val());
            
            // Auto-submit form when any select changes (except sort direction which has its own handler)
            if (selectName !== 'sortDirection') {
                $('#filterForm').submit();
            }
        });
        
        // Handle search input with delay to prevent excessive requests
        let searchTimeout;
        $('#filterForm input[name="search"]').on('input', function() {
            var searchValue = $(this).val();
            console.log('Search input changed:', searchValue);
            
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function() {
                console.log('Auto-submitting search after delay');
                $('#filterForm').submit();
            }, 800); // 800ms delay to allow user to finish typing
        });
        
        // Prevent double submission and show loading state
        $('#filterForm').on('submit', function(e) {
            var $submitBtn = $(this).find('button[type="submit"]');
            var originalText = $submitBtn.text();
            
            // Disable submit button temporarily
            $submitBtn.prop('disabled', true).text('Filtering...');
            
            // Re-enable after 3 seconds (fallback)
            setTimeout(function() {
                $submitBtn.prop('disabled', false).text(originalText);
            }, 3000);
            
            console.log('Filter form submitted');
        });
        
        // Handle pagination clicks with loading indication
        $('.pagination a').on('click', function(e) {
            var $link = $(this);
            if (!$link.parent().hasClass('disabled') && !$link.parent().hasClass('active')) {
                $link.text('Loading...');
                console.log('Pagination clicked:', $link.attr('href'));
            }
        });
        
        // Add smooth scroll to top when pagination is used
        if (window.location.search.includes('pageNumber')) {
            $('html, body').animate({
                scrollTop: $('.job_listing_area').offset().top - 100
            }, 500);
        }
        
        // Add hover effects to job cards
        $('.single_jobs').hover(
            function() {
                $(this).addClass('shadow-lg').css('transform', 'translateY(-2px)');
            },
            function() {
                $(this).removeClass('shadow-lg').css('transform', 'translateY(0)');
            }
        );
        
        // Initialize job count display
        var jobCount = $('.single_jobs').length;
        var totalCount = $('.pagination').length > 0 ? 'of many' : 'total';
        console.log('Showing', jobCount, 'jobs', totalCount);
        
        // Add loading overlay utility
        window.showJobsLoading = function() {
            if ($('#jobsLoadingOverlay').length === 0) {
                $('body').append('<div id="jobsLoadingOverlay" style="position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(255,255,255,0.8);z-index:9999;display:flex;align-items:center;justify-content:center;"><div class="spinner-border text-primary" role="status"><span class="sr-only">Loading...</span></div></div>');
            }
        };
        
        window.hideJobsLoading = function() {
            $('#jobsLoadingOverlay').remove();
        };
        
        console.log('AllJobs page initialized successfully with', jobCount, 'jobs displayed');
    });
})(jQuery);