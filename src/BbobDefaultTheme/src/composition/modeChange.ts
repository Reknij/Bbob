let element = document.documentElement as HTMLElement;

function changeToDark(){
    element.style.setProperty('--theme-border-color', '#323232')
    element.style.setProperty('--theme-background-color', '#32323299')
    element.style.setProperty('--theme-cover-color', '#1d1f21CC')
    element.style.setProperty('--el-text-color-primary', '#ffffff')
    element.style.setProperty('--theme-font-color', '#ffffff')
    element.style.setProperty('--theme-date-color', '#d3d3d3')
    element.style.setProperty('--theme-selected-color', '#a7a7a7')
    element.style.setProperty('--theme-drawer-background-color', '#323232')
}

export {
    changeToDark
}