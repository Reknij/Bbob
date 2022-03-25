# 2022-3-25
## Bbob
- `add` and `remove` command will auto set value to uppper.
- `enable` and `disqus` command will auto set value to uppper. If start with 'plugin-' or 'theme-' will auto add 'bbob-' to front of value.
- new command `run`. will enter mode of target plugin to run custom command.
- optimize help message.
- Load plugin will check interface version.
## BbobDefaultTheme
- article page front have information now.
- fix title position.
## Bbob.Plugin
- refactoring all unsafe field.
- add registerCustomCommand function.

# 2022-3-24
## Bbob
- optimize preview command, support `url` option.

# 2022-3-22 v1.3.4
## Bbob
- update js api.
## BbobDefaultTheme
- use promise function.

# 2022-3-20 v1.3.3
## Bbob
- fix CategoryProcess and TagProcess fix multiple identical tags bug.
- BuildWebArticleJson config add new option.
- optimize `add` command. Support `-r` or `--replace` option, to add plugin with replace no overwrite.
- optimize `add` and `remove` command. Option `-g` have full write `--global`.

# 2022-3-18
## Bbob
- BuildWebArticleJson plugin will assign content hash to 'article'.
- SitemapGenerator fix bug and config default value change.
- Add `ExtraLink` plugin.
## BbobDefaultTheme
- support bbob-plugin-prerender.

# 2022-3-5 v1.3.1
## Bbob
- new command `add` and `remove`. To add the theme or plugin.
- optimize preview.

# 2022-3-4
## Bbob
- `LinkProcess` add condition.
- generate command message optimize.
- Refactoring congiguration.

# 2022-3-3
## Bbob
- `CategoryProcess` support use folder name.
- ConsoleParser optimzie.
- Some build-in plugins support initialize config using `init` command.
- Build-in plugin `ArchiveProcess` require `LinkProcess` to run.
## Bbob.Plugin
- Add PluginCondition

# 2022-3-2
## Bbob
- Add `InitializeBbob` build-in plugin.
- `SitemapGenerator` support set config.
- change hash generate.
- Refactoring code.
## Bbob.Plugin
- Add `Confirm` stage.
- `PluginHelper` optimize.
## BbobDefaultTheme
- optimzie article url show.
- optimize other.
## BbobDocTheme
- optimzie article url show.
- optimize other.
- support `SitemapGenerator`
- support short address.

# 2022-3-1 v1.2.4
## Bbob
- Load plugins support load from nuget packages.
- package as tool.
- add `--version` command.
- add `--help` command.

# 2022-2-27
## Bbob
- meta add `lastBuild`.
- SitemapGenerator can generate sitemap type html. Type txt change to xml.
- remake SiteGenerator plugin. now is require theme info, no need config.
- optimzie loading of Bbob.
## Bbob.Plugin
- add getTheme function

# 2022-2-26
## Bbob
- Optimize meta process
- Add SitemapGenerator build-in plugin.
- Add CopyToDist build-in plugin.
- BuildWebArticleJson build-in plugin now support shortAddress, must enable in config file `"shortAddress": true`.
## Bbob.Plugin
- Plugin can register meta by self.
- PluginHelper optimize.
- CommandOperation add `RunMeAgain`.

# 2022-2-24
## Bbob
- All assets of generation now migrate to one folder 'bbob.assets'.
- optimize path of app.
## Bbob.Plugin
- Optimize function of PluginHelper
## BbobDefaultTheme
- Blog title color change
## BbobDocTheme
- Blog title color change

# 2022-2-23
## Bbob
- Add preview command to preview your blog, must generate first.
- Add reset-config command, use with given name to reset target config to default.
- Add SkipIfDraft build-in plugin, if article front matter have set 'draft' and value is true, generate will skip it.
- Distribution now have archives.
- Bbob js api new function `getLinkInfosWithArchiveAddress`.
- Add Bbob js blog.archives. It is array of ArticleYear object.
## BbobDefaultTheme
- Add archives.
## Bbob.Plugin
- Command now can set command result, to control command operation.
- Add extensions to give some functions helper develop plugins. Etc. check object exists property.

# 2022-2-21
## Bbob
- If plugin no assign name in plugin.json, it will default name folder of plugin.
## Bbob.Plugin
- PluginJson add 'entry' section, it is entry whose file is plugin entry. Default is 'MainPlugin.dll'
- PluginJson description and author now have default info.

# 2022-2-20
## Bbob
- change sort article of SortData plugin. Now is sort by order instead of name.
- plugin process variable type now is `dynamic` instead of `LinkInfo`
- plugin now have two variable to process, it is `article` and `link`. `article` is about article data, and `link` is about links.
- plugin will serialize all fields of `article` and `link` now.
- MarkdownParser plugin no provide `getYamlObject` function now.
- config.publicPath change to config.base

# 2022-2-18
## Bbob
- add deploy command
- add build-in plugin GitDeploy, can easy deploy blog to git.
- fix config.allLink recheck wrong logical
- fix next file hash and url.
## Bbob.Plugin
- move the PluginJson, plugin info.
- interface add deploy command.
- `PluginHelper.printConsole()` let plugin print message.
- `PluginHelper.ExecutingPlugin` can get info of plugin.
- `getPluginJsonConfig` and `savePluginJsonConfig` have default name now. Default reference to the name of executing plugin.
## BbobDefaultTheme
- import modules with cdn
- fix scroll down load
- optimize categories and tags
- optimize toc auto close
## BbobDocTheme
- import modules with cdn