[![Nuget](https://img.shields.io/nuget/v/bbob?label=Bbob&style=flat-square)](https://www.nuget.org/packages/Bbob/)[![Nuget](https://img.shields.io/nuget/dt/bbob?style=flat-square)](https://www.nuget.org/packages/Bbob/)  [![Nuget](https://img.shields.io/nuget/v/bbob.plugin?label=Bbob.Plugin&style=flat-square)](https://www.nuget.org/packages/Bbob.Plugin/)[![Nuget](https://img.shields.io/nuget/dt/bbob.plugin?style=flat-square)](https://www.nuget.org/packages/Bbob/)  [![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://paypal.me/JinkerLeong?country.x=MY&locale.x=en_US)

# Bbob 
One similar static blog framework no need server service.

# Install
Requires .Net 6+

Now only support install from nuget.
```
dotnet tool install -g bbob
```

# How to use?
About usage and plugin develop, please see the [Documents](https://reknij.github.io/Bbob.Doc/)

# Why make it?
Feel funny

# Last update 2022-5-14 v1.5.1.1
# 2022-5-14
## Bbob
- fix `GitDeploy`.

# 2022-8-3 v1.5.2
## Bbob
- `BuildWebArticleJson` will add information relative between articles now.
- fix `LinkProcess` will add content hash code to `bbob.js`.
- `LinkProcess` will add hash code of content to article json now. 
- `LinkProcess` registered object `links` now is now modified. It will also fix article property changed bug.

## Bbob.Plugin
- Plugin can register event program exited now.

## BbobDefaultTheme
- article page have relative article address 'next' and 'previous'.

# Recommended plugins
- [bbob-plugin-disqus](https://github.com/Reknij/bbob-plugin-disqus) Let your article have comment.
- [bbob-plugin-prerender](https://github.com/Reknij/bbob-plugin-prerender) Generate static page for seo.
- [bbob-plugin-mathjax](https://github.com/Reknij/bbob-plugin-mathjax) Let your blog articles support display mathematics syntax.
