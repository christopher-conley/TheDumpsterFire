// @name         Let Me Multitask, Bro
// @author       Christopher Conley
// @copyright    Copyright (C) 2023 Christopher Conley
// @license      GPL-2.0-only
// @version      0.2.1
// @description  A Chrome extension that allows picture-in-picture mode to be used for videos, even if the website has disabled it.
//
// Picture in picture icon by Ria Fitriana from https://thenounproject.com/browse/icons/term/picture-in-picture/ Noun Project (CC BY 3.0)
//

console.log('LMMB: Let Me Multitask, Bro extension triggered.')

var poller = setInterval(function () {
    if (document.querySelector('video') !== null) {
        clearInterval(poller);
        console.log("LMMB: Found video element.");

        var videos = document.querySelectorAll('video');

        videos.forEach(video => {
            if (video.hasAttribute('disablepictureinpicture')) {
                console.log('LMMB: Removing disablepictureinpicture attribute from video element.');
                video.removeAttribute('disablepictureinpicture');
            }
        });
    }
}, 50);