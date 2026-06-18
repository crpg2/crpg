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
  putItemsByIdThemes,
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

// Item theme tagging (admin). Single = set/replace, bulk = add/remove.
export const setItemThemes = async (itemId: string, themeIds: number[]): Promise<ItemViewModel> =>
  (await putItemsByIdThemes({ path: { id: itemId }, body: { themeIds } })).data!

export const addThemesToItems = (itemIds: string[], themeIds: number[]) =>
  putItemsThemes({ body: { itemIds, themeIds } })

export const removeThemesFromItems = (itemIds: string[], themeIds: number[]) =>
  deleteItemsThemes({ body: { itemIds, themeIds } })
