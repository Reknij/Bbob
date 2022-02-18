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