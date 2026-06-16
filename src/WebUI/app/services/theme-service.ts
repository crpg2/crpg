import type { 
    CreateThemeCommand, 
    CreateThemeEventCommand, 
    ThemeEventViewModel, 
    ThemeViewModel, 
    UpdateThemeCommand, 
    UpdateThemeEventCommand,
 } from '#api' 

 import { 
    deleteThemesById, 
    deleteThemesEventsById, 
    getThemes as _getThemes, 
    getThemesEvents as _getThemesEvents, 
    postThemes, 
    postThemesEvents, 
    putThemes, 
    putThemesEvents, 
 } from '#api/sdk.gen' 

 export const getThemes = async (): Promise<ThemeViewModel[]> => (await _getThemes({})).data! 

 export const createTheme = (body: CreateThemeCommand) => postThemes({ body }) 

 export const updateTheme = (body: UpdateThemeCommand) => putThemes({ body }) 

 export const deleteTheme = (id: number) => deleteThemesById({ path: { id } }) 

 export const getThemeEvents = async (): Promise<ThemeEventViewModel[]> => (await _getThemesEvents({})).data! 

 export const createThemeEvent = (body: CreateThemeEventCommand) => postThemesEvents({ body }) 

 export const updateThemeEvent = (body: UpdateThemeEventCommand) => putThemesEvents({ body }) 

 export const deleteThemeEvent = (id: number) => deleteThemesEventsById({ path: { id } })