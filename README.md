# Bbob
One serverless blog framework.

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

# Last update 2022-3-29 v1.4.5
## Bbob
- load plugin from nuget packages match name now is upper
- fix command `add -a` option can't detect
- MarkdownParser build-in plugin now use advanced extension. etc. tables, strikethrough, task list and more.

## BbobDefaultTheme
- add icons.
- optimize color.
- add back top button.
- toc button refactoring.

# Recommended plugins
- [bbob-plugin-disqus](https://github.com/Reknij/bbob-plugin-disqus) Let your article have comment.
- [bbob-plugin-prerender](https://github.com/Reknij/bbob-plugin-prerender) Generate static page for seo.
- [bbob-plugin-mathjax](https://github.com/Reknij/bbob-plugin-mathjax) Let your blog articles support display mathematics syntax.
