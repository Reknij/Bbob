# 2022-2-23 13:40:00
## Bbob
- Add preview command to preview your blog, must generate first.

# 2022-2-21 19:51:00
## Bbob
- If plugin no assign name in plugin.json, it will default name folder of plugin.
## Bbob.Plugin
- PluginJson add 'entry' section, it is entry whose file is plugin entry. Default is 'MainPlugin.dll'
- PluginJson description and author now have default info.

# 2022-2-20 20:55:00
## Bbob
- change sort article of SortData plugin. Now is sort by order instead of name.
- plugin process variable type now is `dynamic` instead of `LinkInfo`
- plugin now have two variable to process, it is `article` and `link`. `article` is about article data, and `link` is about links.
- plugin will serialize all fields of `article` and `link` now.
- MarkdownParser plugin no provide `getYamlObject` function now.
- config.publicPath change to config.base
# 2022-2-18 23:42:00
## Bbob
- add deploy command
- add build-in plugin GitDeploy, can easy deploy blog to git.
## Bbob.Plugin
- move the PluginJson, plugin info.
- interface add deploy command.
- `PluginHelper.printConsole()` let plugin print message.
- `PluginHelper.ExecutingPlugin` can get info of plugin.
- `getPluginJsonConfig` and `savePluginJsonConfig` have default name now. Default reference to the name of executing plugin.

# 2022-2-18 18:42:00
## Bbob
- fix config.allLink recheck wrong logical
- fix next file hash and url.
## BbobDefaultTheme
- import modules with cdn
- fix scroll down load
- optimize categories and tags
- optimize toc auto close
## BbobDocTheme
- import modules with cdn