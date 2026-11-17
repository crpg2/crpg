import type {
  CreateThemeCommand,
  CreateThemeEventCommand,
  ItemViewModel,
  ThemeEventViewModel,
  ThemeViewModel,
  UpdateThemeCommand,
  UpdateThemeEventCommand,
} from '#api'

import {
  getThemes as _getThemes,
  getThemesEvents as _getThemesEvents,
  deleteItemsThemes,
  deleteThemesById,
  deleteThemesEventsById,
  postThemes,
  postThemesEvents,
  putItemsByBaseIdThemes,
  putItemsThemes,
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

// Item theme tagging (admin). Themes apply to a whole item family (BaseId = all rank variants).
// Single = set/replace, bulk = add/remove.
export const setItemThemes = async (baseId: string, themeIds: number[]): Promise<ItemViewModel> =>
  (await putItemsByBaseIdThemes({ path: { baseId }, body: { themeIds } })).data!

export const addThemesToItems = (baseIds: string[], themeIds: number[]) =>
  putItemsThemes({ body: { baseIds, themeIds } })

export const removeThemesFromItems = (baseIds: string[], themeIds: number[]) =>
  deleteItemsThemes({ body: { baseIds, themeIds } })
