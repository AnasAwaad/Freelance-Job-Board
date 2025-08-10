$(document).ready(function () {
    $('.brand-carousel').owlCarousel({
        items: 4,
        loop: true,
        margin: 10,
        autoplay: true,
        autoplayTimeout: 2000,
        autoplayHoverPause: true,
        smartSpeed: 800,
        responsive: {
            0: { items: 2 },
            600: { items: 3 },
            1000: { items: 4 }
        },
        dots: false
    });
});