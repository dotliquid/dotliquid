/**
* Name: piroBox v.1.2.1
* Date: November 2009
* Autor: Diego Valobra (http://www.pirolab.it),(http://www.diegovalobra.com)
* Version: 1.2.1
* Licence: CC-BY-SA http://creativecommons.org/licenses/by-sa/2.5/it/
**/		
(function($) {
	$.fn.piroBox = function(opt) {
		opt = jQuery.extend({
		my_speed : null,
		close_speed : 300,
		bg_alpha : 0.5,
		scrollImage : null,
		pirobox_next : 'piro_next_out',
		pirobox_prev :  'piro_prev_out',
		radius : 4,
		close_all : '.piro_close,.piro_overlay',
		slideShow : null,
		slideSpeed : null //slideshow duration in seconds
		}, opt);		
		function start_pirobox() {
			var corners = 
				'<tr>'+					   
				'<td colspan="3" class="pirobox_up"></td>'+
				'</tr>'+	
				'<tr>'+	
				'<td class="t_l"></td>'+
				'<td class="t_c"></td>'+
				'<td class="t_r"></td>'+
				'</tr>'+
				'<tr>'+
				'<td class="c_l"></td>'+
				'<td class="c_c"><span><span></span></span><div></div></td>'+
				'<td class="c_r"></td>'+
				'</tr>'+
				'<tr>'+
				'<td class="b_l"></td>'+
				'<td class="b_c"></td>'+
				'<td class="b_r"></td>'+
				'</tr>'+
				'<tr>'+
				'<td colspan="3" class="pirobox_down"></td>'+
				'</tr>';
			var window_height =  $(window).height();
			var bg_overlay = $(jQuery('<div class="piro_overlay"></div>').hide().css({'opacity':+opt.bg_alpha,'height':window_height+'px'}));
			var main_cont = $(jQuery('<table class="pirobox_content" cellpadding="0" cellspacing="0"></table>'));
			var caption = $(jQuery('<div class="caption"></div>').css({'opacity':'0.8','-moz-border-radius':opt.radius+'px','-khtml-border-radius':opt.radius+'px','-webkit-border-radius':opt.radius+'px','border-radius':opt.radius+'px'}));
			var piro_nav = $(jQuery('<div class="piro_nav"></div>'));
			var piro_close = $(jQuery('<div class="piro_close"></div>'));
			var piro_play = $(jQuery('<a href="#play" class="play"></a>'));
			var piro_stop = $(jQuery('<a href="#stop" class="stop"></a>'));
			var piro_prev = $(jQuery('<a href="#prev" class="'+opt.pirobox_prev+'"></a>'));
			var piro_next = $(jQuery('<a href="#next" class="'+opt.pirobox_next+'"></a>'));				
			$('body').append(bg_overlay).append(main_cont);
			main_cont.append(corners);
			$('.pirobox_up').append(piro_close);
			$('.pirobox_down').append(piro_nav);
			$('.c_c').append(piro_play);
			piro_play.hide();
			piro_nav.append(piro_prev).append(piro_next).append(caption);
			if(piro_prev.is('.piro_prev_out') || piro_next.is('.piro_next_out')){
				$('body').append(piro_prev).append(piro_next);
				piro_prev.add(piro_next).hide()
			}else{
				piro_nav.append(piro_prev).append(piro_next);
			}
			var my_nav_w = piro_prev.width();
			main_cont.hide();
			var my_gall_classes = $("a[class^='pirobox_gall']");
			var map = new Object();
				for (var i=0; i<my_gall_classes.length; i++) {
					var it=$(my_gall_classes[i])
					map['a.'+it.attr('class')]=0;
				}
			var gall_settings = new Array();
				for (var key in map) {
					gall_settings.push(key);
					if($(key).length === 1){//check on set of images
					alert('For single image is recommended to use class pirobox');
					$(key).css('border','2px dotted red');
					}
				}
				for (var i=0; i<gall_settings.length; i++) {
					$(gall_settings[i]).each(function(rel){this.rel = rel+1 + "&nbsp;of&nbsp;" + $(gall_settings[i]).length;});
					var add_first = $(gall_settings[i]+':first').addClass('first');
					var add_last = $(gall_settings[i]+':last').addClass('last');
				}						
			$(my_gall_classes).each(function(rev){this.rev = rev+0});
			var piro_gallery = $(my_gall_classes);
			var piro_single = $('a.pirobox');
			$.fn.fixPNG = function() {
				return this.each(function () {
					var image = $(this).css('backgroundImage');
					if (image.match(/^url\(["']?(.*\.png)["']?\)$/i)) {
						image = RegExp.$1;
						$(this).css({
							'backgroundImage': 'none',
							'filter': "progid:DXImageTransform.Microsoft.AlphaImageLoader(enabled=true, sizingMethod=" + ($(this).css('backgroundRepeat') == 'no-repeat' ? 'crop' : 'scale') + ", src='" + image + "')"
						}).each(function () {
							var position = $(this).css('position');
							if (position != 'absolute' && position != 'relative')
								$(this).css('position', 'relative');
						});
					}
				});
			};
			$(window).resize(function(){
				var new_w_bg = $(window).height();
				bg_overlay.css({'visibility':'visible','height':+ new_w_bg +'px'});				  
			});	
			piro_prev.add(piro_next).bind('click',function(c) {
				c.preventDefault();
				var image_count = parseInt($(piro_gallery).filter('.item').attr('rev'));
				var start = $(this).is('.piro_prev_out,.piro_prev') ? $(piro_gallery).eq(image_count - 1) : $(piro_gallery).eq(image_count + 1);
				start.click();
				piro_close.add(caption).add(piro_next).add(piro_prev).css('visibility','hidden');
			});
			piro_single.each(function(d) {
				var item = $(this);
				item.bind('click',function(d) {
					d.preventDefault();
					piro_open(item.attr('href'));
					var this_url = item.attr('href');
						var descr = item.attr('title');		
						if( descr == ""){
						caption.html('<p>'+ this_url+'<a href='+ this_url +' class="link_to" target="_blank" title="Open Image in a new window"></a></p>');
						}else{
						caption.html('<p>'+ descr+'<a href='+ this_url +' class="link_to" target="_blank" title="Open Image in a new window"></a></p>');
						}
					$('.c_c').addClass('unique');
					piro_next.add(piro_prev).add(piro_close).add(caption).hide();
					$('.play').remove();
				});
			});
			$(piro_gallery).each(function(array) {
					var item = $(this);
					item.bind('click',function(c) {
						c.preventDefault();
						piro_open(item.attr('href'));
						var this_url = item.attr('href');
						var descr = item.attr('title');	
						var number = item.attr('rel');
						if( descr == ""){
						caption.html('<p>'+ this_url+'<em class="number">' + number + '</em><a href='+ this_url +' class="link_to" target="_blank" title="Open Image in a new window"></a></p>');
						}else{
						caption.html('<p>'+ descr+'<em class="number">' + number + '</em><a href='+ this_url +' class="link_to" target="_blank" title="Open Image in a new window"></a></p>');
						}
						if(item.is('.last')){
							$('.number').css('text-decoration','underline');	  
						}else{
							$('.number').css('text-decoration','none');
							}				
						if(item.is('.first')){
							piro_prev.hide();
							piro_next.show();		
						}else{
							piro_next.add(piro_prev).show();		  
						}
						if(item.is('.last')){
							piro_prev.show();
							piro_next.hide();			  
						}
						if(item.is('.last') && item.is('.first') ){
							piro_prev.add(piro_next).hide();
							$('.number').hide();
							piro_play.remove();
						}					
							$(piro_gallery).filter('.item').removeClass('item');
							item.addClass('item');
							$('.c_c').removeClass('unique');		
					});
				});
				var piro_open = function(my_url) {
					piro_play.add(piro_stop).hide();
					piro_close.add(caption).add(piro_next).add(piro_prev).css('visibility','hidden');
					if(main_cont.is(':visible')) {
						$('.c_c div').children().fadeOut(300, function() {
							$('.c_c div').children().remove();
							load_img(my_url);
						});
					} else {
						$('.c_c div').children().remove();
						main_cont.show();
						bg_overlay.fadeIn(300,function(){
							load_img(my_url);

						});
					}
				}
				var load_img = function(my_url) {
				if(main_cont.is('.loading')) {return;}
				main_cont.addClass('loading');
				var img = new Image();
				img.onerror = function (){
					var main_cont_h = $(main_cont).height();
					main_cont.css({marginTop : parseInt($(document).scrollTop())-(main_cont_h/1.9)});
				  $('.c_c div').append('<p class="err_mess">There seems to be an Error:&nbsp;<a href="#close" class="close_pirobox">Close Pirobox</a></p>');
					$('.close_pirobox').bind('click',function() {
						$('.err_mess').remove();
						main_cont.add(bg_overlay).fadeOut(opt.close_speed);
						main_cont.removeClass('loading');
						$('.c_c').append(piro_play);
						return false;
					});
				}
				img.onload = function() {
					var imgH = img.height;
					var imgW = img.width;
					var main_cont_h = $(main_cont).height();
					var w_H = $(window).height();
					var w_W = $(window).width();
					
					if(imgH+100 > w_H || imgW+100 > w_W){
						var new_img_W = imgW;
						var new_img_H = imgH;
						var _x = (imgW + 250)/w_W;
						var _y = (imgH + 250)/w_H;
						if ( _y > _x ){
							new_img_W = Math.round(imgW * (1/_y));
							new_img_H = Math.round(imgH * (1/_y));
						} else {
							new_img_W = Math.round(imgW * (1/_x));
							new_img_H = Math.round(imgH * (1/_x));
						}
						imgH += new_img_H;
						imgW += new_img_W;
						$(img).height(new_img_H).width(new_img_W).hide();
						$('.c_c div').animate({height:new_img_H+'px',width:new_img_W+'px'},opt.my_speed);				
						main_cont.animate({
						height : (new_img_H+20) + 'px' ,
						width : (new_img_W+20) + 'px' , 
						marginLeft : '-' +((new_img_W)/2+10) +'px',
						marginTop : parseInt($(document).scrollTop())-(new_img_H/1.9)-20},opt.my_speed, function(){	
						$('.piro_nav,.caption').css({width:(new_img_W)+'px'});
						$('.piro_nav').css('margin-left','-'+(new_img_W+5)/2+'px');
							var caption_height = caption.height();
							caption.css({'bottom':'-'+(caption_height+5)+'px'});
							$('.c_c div').append(img);
							piro_close.css('display','block');
							piro_next.add(piro_prev).add(piro_close).css('visibility','visible');
							caption.css({'visibility':'visible','display':'block'});
								$(img).show().fadeIn(300);
									main_cont.removeClass('loading');
									if(opt.slideShow == 'slideshow'){
									   piro_play.add(piro_stop).show();
									}else{
										 piro_play.add(piro_stop).hide();
									}									
							});
				}else{
					$(img).height(imgH).width(imgW).hide();
						$('.c_c div').animate({height:imgH+'px',width:imgW+'px'},opt.my_speed);
						main_cont.animate({
						height : (imgH+20) + 'px' ,
						width : (imgW+20) + 'px' , 
						marginLeft : '-' +((imgW)/2+10) +'px',
						marginTop : parseInt($(document).scrollTop())-(imgH/1.9)-20},opt.my_speed, function(){
						$('.piro_nav,.caption').css({width:(imgW)+'px'});
						$('.piro_nav').css('margin-left','-'+(imgW+5)/2+'px');
							var caption_height = caption.height();
							caption.css({'bottom':'-'+(caption_height+5)+'px'});
							$('.c_c div').append(img);					
							piro_close.css('display','block');
							piro_next.add(piro_prev).add(piro_close).css('visibility','visible');
							caption.css({'visibility':'visible','display':'block'});
								$(img).fadeIn(300);
									main_cont.removeClass('loading');
									if(opt.slideShow == 'slideshow'){
									   piro_play.add(piro_stop).show();
									}else{
										 piro_play.add(piro_stop).hide();
									}
							});			
						}
			  		}
					img.src = my_url;
					var win_h = $(window).height();
					var nav_h = $('.piro_prev_out').height();
					$('.piro_prev_out').add('.piro_next_out').css({marginTop : parseInt($(document).scrollTop())+(win_h/nav_h-125)});	
					$('.caption p').css({'-moz-border-radius':opt.radius+'px','-khtml-border-radius':opt.radius+'px','-webkit-border-radius':opt.radius+'px','border-radius':opt.radius+'px'});	  
					piro_stop.bind('click',function(x){
						x.preventDefault();
						clearTimeout(timer);
						$(piro_gallery).children().removeAttr('class');
						$('.stop').remove();
						$('.c_c').append(piro_play);
						piro_next.add(piro_prev).css('width',my_nav_w+'px');
					});
					piro_play.bind('click',function(w){
						w.preventDefault();
						clearTimeout(timer);
						if($(img).is(':visible')){
						$(piro_gallery).children().addClass(opt.slideShow);
						$('.play').remove();
						$('.c_c').append(piro_stop);
						}
						piro_next.add(piro_prev).css({'width':'0px'});
						return slideshow();
					});
				  $(opt.close_all).bind('click',function(c) {
					clearTimeout(timer);
					if($(img).is(':visible')){
						c.preventDefault();
						piro_close.add(bg_overlay).add(main_cont).add(caption).add(piro_next).add(piro_prev).fadeOut(opt.close_speed);
						main_cont.removeClass('loading');
						$(piro_gallery).children().removeAttr('class');
						piro_next.add(piro_prev).css('width',my_nav_w+'px').hide();
						$('.stop').remove();
						$('.c_c').append(piro_play);
						piro_play.hide();
					  }
				  });	
					function slideshow(){
					clearTimeout(timer);
					if( $(piro_gallery).filter('.item').is('.last')){
						$(piro_gallery).children().removeAttr('class');
						piro_next.add(piro_prev).css('width',my_nav_w+'px');

						$('.stop').remove();
						$('.c_c').append(piro_play);
						piro_play.hide();
					}else if($(piro_gallery).children().is('.' + opt.slideShow )){
						piro_next.click();
						}
					}
					var timer = setInterval(slideshow,opt.slideSpeed*1000 );
					$().bind("keydown", function (c) {
					if (c.keyCode === 27) {
						c.preventDefault();
						if($(img).is(':visible') || $('.c_c>div>p>a').is('.close_pirobox')){
					  piro_close.add(bg_overlay).add(main_cont).add(caption).add(piro_next).add(piro_prev).fadeOut(opt.close_speed);
					  main_cont.removeClass('loading');
						clearTimeout(timer);
					  	$(piro_gallery).children().removeAttr('class');
						$('.stop').remove();
						$('.c_c').append(piro_play);
						piro_next.add(piro_prev).css('width',my_nav_w+'px');
					  $(piro_gallery).add(piro_single).children().fadeTo(100,1);
							}
						}
					}).bind("keydown", function(e) {
						if ($('.c_c').is('.unique') || $('.item').is('.first')){
						}else if($('.c_c').is('.c_c')&&(e.keyCode === 37)) {
							e.preventDefault();
							if($(img).is(':visible')){
							clearTimeout(timer);
							$(piro_gallery).children().removeAttr('class');
							$('.stop').remove();
							$('.c_c').append(piro_play);
							piro_next.add(piro_prev).css('width',my_nav_w+'px');
							piro_prev.click();
							} 
						}
						if ($('.c_c').is('.unique') || $('.item').is('.last')){
						}else if($('.c_c').is('.c_c')&&(e.keyCode === 39)) {
							e.preventDefault();
							if($(img).is(':visible')){
							clearTimeout(timer);
							$(piro_gallery).children().removeAttr('class');
							$('.stop').remove();
							$('.c_c').append(piro_play);
							piro_next.add(piro_prev).css('width',my_nav_w+'px');
							piro_next.click();
							} 
						}
					});
					$.browser.msie6 =($.browser.msie && /MSIE 6\.0/i.test(window.navigator.userAgent));
					if( $.browser.msie6 && !/MSIE 8\.0/i.test(window.navigator.userAgent)) {
						$('.t_l,.t_c,.t_r,.c_l,.c_r,.b_l,.b_c,.b_r,a.piro_next, a.piro_prev,a.piro_prev_out,a.piro_next_out,.c_c,.piro_close,a.play,a.stop').fixPNG();
						var ie_w_h =  $(document).height();
						bg_overlay.css('height',ie_w_h+ 'px'); 
					}
					if( $.browser.msie) {
					opt.close_speed = 0;
					}
					function scrollImage (){
						if($(main_cont).is(':visible')){
							window.onscroll = function (){
								var main_cont_h = $(main_cont).height();
								main_cont.css({
								marginTop : parseInt($(this).scrollTop())-(main_cont_h/1.9)-10
								});
								var Nwin_h = $(window).height();
								var Nnav_h = $('.piro_prev_out').height();
								$('.piro_prev_out').add('.piro_next_out').css({marginTop : parseInt($(document).scrollTop())+(Nwin_h/Nnav_h-125)});				
								}
							}
						}
					if(opt.scrollImage == true){
						return scrollImage();
					}

				}
			}

		start_pirobox();
	}
})(jQuery);