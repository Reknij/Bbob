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

# 2022-5-11
## Bbob
- Replace all unknown characters.
- `GitDeploy` refactoring some codes
- `GitDeploy` now default will no ping sitemap.
- `GitDeploy` ping sitemap support `Bing` search engine.
- `BuildWebArticleJson` remove `shortAddressEndWithSlash` config option.
- `BuildWebArticleJson` and `SitemapGenerator` refactoring `articleBaseUrl` of theme info.
## Bbob.Plugin
- Replace all unknown characters.
- fix `printConsole` no newline in multithreading.
- will cache plugin config and theme info to optimize get speed.
- theme support include plugins.

# Recommended plugins
- [bbob-plugin-disqus](https://github.com/Reknij/bbob-plugin-disqus) Let your article have comment.
- [bbob-plugin-prerender](https://github.com/Reknij/bbob-plugin-prerender) Generate static page for seo.
- [bbob-plugin-mathjax](https://github.com/Reknij/bbob-plugin-mathjax) Let your blog articles support display mathematics syntax.
