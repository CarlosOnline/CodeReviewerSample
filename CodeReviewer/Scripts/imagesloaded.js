/* qTip2 v2.0.1-240 viewport | qtip2.com | Licensed MIT, GPL | Mon Oct 21 2013 08:20:49 */
/*!
 * imagesLoaded v3.0.2
 * JavaScript is all like "You images are done yet or what?"
 */
(function(t){"use strict";function e(t,e){for(var i in e)t[i]=e[i];return t}function i(t){return"[object Array]"===h.call(t)}function s(t){var e=[];if(i(t))e=t;else if("number"==typeof t.length)for(var s=0,n=t.length;n>s;s++)e.push(t[s]);else e.push(t);return e}function n(t,i){function n(t,i,r){if(!(this instanceof n))return new n(t,i);"string"==typeof t&&(t=document.querySelectorAll(t)),this.elements=s(t),this.options=e({},this.options),"function"==typeof i?r=i:e(this.options,i),r&&this.on("always",r),this.getImages(),o&&(this.jqDeferred=new o.Deferred);var a=this;setTimeout(function(){a.check()})}function h(t){this.img=t}n.prototype=new t,n.prototype.options={},n.prototype.getImages=function(){this.images=[];for(var t=0,e=this.elements.length;e>t;t++){var i=this.elements[t];"IMG"===i.nodeName&&this.addImage(i);for(var s=i.querySelectorAll("img"),n=0,o=s.length;o>n;n++){var r=s[n];this.addImage(r)}}},n.prototype.addImage=function(t){var e=new h(t);this.images.push(e)},n.prototype.check=function(){function t(t,n){return e.options.debug&&a&&r.log("confirm",t,n),e.progress(t),i++,i===s&&e.complete(),!0}var e=this,i=0,s=this.images.length;if(this.hasAnyBroken=!1,!s)return this.complete(),void 0;for(var n=0;s>n;n++){var o=this.images[n];o.on("confirm",t),o.check()}},n.prototype.progress=function(t){this.hasAnyBroken=this.hasAnyBroken||!t.isLoaded,this.emit("progress",this,t),this.jqDeferred&&this.jqDeferred.notify(this,t)},n.prototype.complete=function(){var t=this.hasAnyBroken?"fail":"done";if(this.isComplete=!0,this.emit(t,this),this.emit("always",this),this.jqDeferred){var e=this.hasAnyBroken?"reject":"resolve";this.jqDeferred[e](this)}},o&&(o.fn.imagesLoaded=function(t,e){var i=new n(this,t,e);return i.jqDeferred.promise(o(this))});var l={};return h.prototype=new t,h.prototype.check=function(){var t=l[this.img.src];if(t)return this.useCached(t),void 0;if(l[this.img.src]=this,this.img.complete&&void 0!==this.img.naturalWidth)return this.confirm(0!==this.img.naturalWidth,"naturalWidth"),void 0;var e=this.proxyImage=new Image;i.bind(e,"load",this),i.bind(e,"error",this),e.src=this.img.src},h.prototype.useCached=function(t){if(t.isConfirmed)this.confirm(t.isLoaded,"cached was confirmed");else{var e=this;t.on("confirm",function(t){return e.confirm(t.isLoaded,"cache emitted confirmed"),!0})}},h.prototype.confirm=function(t,e){this.isConfirmed=!0,this.isLoaded=t,this.emit("confirm",this,e)},h.prototype.handleEvent=function(t){var e="on"+t.type;this[e]&&this[e](t)},h.prototype.onload=function(){this.confirm(!0,"onload"),this.unbindProxyEvents()},h.prototype.onerror=function(){this.confirm(!1,"onerror"),this.unbindProxyEvents()},h.prototype.unbindProxyEvents=function(){i.unbind(this.proxyImage,"load",this),i.unbind(this.proxyImage,"error",this)},n}var o=t.jQuery,r=t.console,a=r!==void 0,h=Object.prototype.toString;"function"==typeof define&&define.amd?define(["eventEmitter","eventie"],n):t.imagesLoaded=n(t.EventEmitter,t.eventie)})(window);
//@ sourceMappingURL=http://qtip2.com/v/nightly//tmp/tmp-71399fh7e8g/imagesloaded.min.map