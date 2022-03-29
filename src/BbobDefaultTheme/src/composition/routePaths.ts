import { language} from '../Languages/LanguageHelper';
const paths = [
    {
        name: language.home,
        path: '/'
    },
    {
        name: language.articles,
        path: '/articles/scroll'
    },
    {
        name: language.archives,
        path: '/filter/archives'
    },
    {
        name: language.categories,
        path: '/filter/categories'
    },
    {
        name: language.tags,
        path: '/filter/tags'
    },
    {
        name: language.about,
        path: '/about'
    }
]

export {
    paths
}