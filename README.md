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

# Last update 2023-2-3 v1.6.0
## Bbob
- Add `dev` command to generate files for development.
- bbob.js api add drawHtmlToElement function to easy draw html from string to target element and execute script.
- fix `GitDeploy` bugs.
- `add` command support add from list file `./addlist.txt`
- `remove` command support remove all plugin or theme(except `default`).
- `BuildWebArticleJson` plugin now remove config support.

## BbobDefaultTheme
- remove `articleBaseUrlShort` data from theme info. Now get article through the `id` instead `address`.

# Recommended plugins
- [bbob-plugin-disqus](https://github.com/Reknij/bbob-plugin-disqus) Let your article have comment.
- [bbob-plugin-prerender](https://github.com/Reknij/bbob-plugin-prerender) Generate static page for seo.
- [bbob-plugin-mathjax](https://github.com/Reknij/bbob-plugin-mathjax) Let your blog articles support display mathematics syntax.
