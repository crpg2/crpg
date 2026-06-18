import type { ThemeEventViewModelIListResult, ThemeViewModelIListResult } from '~~/generated/api' 

 import { describe, expect, it, vi } from 'vitest' 

 import { 
    createTheme, 
    createThemeEvent, 
    deleteTheme, 
    deleteThemeEvent, 
    getThemeEvents, 
    getThemes, 
    updateTheme, 
    updateThemeEvent, 
 } from '~/services/theme-service' 

 const { 
    _getThemes, 
    _postThemes, 
    _putThemes, 
    _deleteThemesById, 
    _getThemesEvents, 
    _postThemesEvents, 
    _putThemesEvents, 
    _deleteThemesEventsById, 
 } = vi.hoisted(() => ({ 
    _getThemes: vi.fn(), 
    _postThemes: vi.fn(), 
    _putThemes: vi.fn(), 
    _deleteThemesById: vi.fn(), 
    _getThemesEvents: vi.fn(), 
    _postThemesEvents: vi.fn(), 
    _putThemesEvents: vi.fn(), 
    _deleteThemesEventsById: vi.fn(), 
 })) 

 vi.mock('#api/sdk.gen', () => ({ 
    getThemes: _getThemes, 
    postThemes: _postThemes, 
    putThemes: _putThemes, 
    deleteThemesById: _deleteThemesById, 
    getThemesEvents: _getThemesEvents, 
    postThemesEvents: _postThemesEvents, 
    putThemesEvents: _putThemesEvents, 
    deleteThemesEventsById: _deleteThemesEventsById, 
 })) 

 describe('theme service', () => { 
    describe('themes', () => { 
        it('getThemes unwraps the result data', async () => { 
            _getThemes.mockResolvedValueOnce({ 
            data: [ 
                { id: 1, name: 'Viking' }, 
                { id: 2, name: 'Steppe' }, 
            ], 
            errors: null, 
            } satisfies ThemeViewModelIListResult) 
        
            const themes = await getThemes() 
        
            expect(_getThemes).toHaveBeenCalledWith({}) 
            expect(themes).toEqual([ 
                { id: 1, name: 'Viking' }, 
                { id: 2, name: 'Steppe' }, 
            ]) 
        }) 

        it('createTheme posts the command body', async () => { 
            _postThemes.mockResolvedValueOnce({ data: { id: 3, name: 'Desert' }, errors: null }) 
            
            await createTheme({ name: 'Desert' }) 
            
            expect(_postThemes).toHaveBeenCalledWith({ body: { name: 'Desert' } }) 
        }) 
    
        it('updateTheme puts to the id route with the body', async () => { 
            _putThemes.mockResolvedValueOnce({ data: { id: 3, name: 'Dunes' }, errors: null }) 
            
            await updateTheme({ id: 3, name: 'Dunes' }) 
            
            expect(_putThemes).toHaveBeenCalledWith( { body: { id: 3, name: 'Dunes' } }) 
        }) 

        it('deleteTheme deletes by id', async () => { 
            _deleteThemesById.mockResolvedValueOnce({ data: undefined, errors: null }) 

            await deleteTheme(7) 

            expect(_deleteThemesById).toHaveBeenCalledWith({ path: { id: 7 } }) 
        }) 
    }) 

    describe('theme events', () => { 
        const eventBody: Parameters<typeof createThemeEvent>[0] = { 
            name: 'Viking Night', 
            themeId: 1, 
            goldMultiplier: 1.5, 
            expMultiplier: 2, 
            activeFromUtc: new Date('2026-06-16T18:00:00.000Z'), 
            activeUntilUtc: null, 
            requiredEquipmentSlotsMatchingTheme: ['Body', 'Weapon'],
            minimumThemedItemsEquipped: 2,
        }

        it('getThemeEvents unwraps the result data', async () => { 
            _getThemesEvents.mockResolvedValueOnce({ 
            data: [], 
            errors: null, 
            } satisfies ThemeEventViewModelIListResult) 
        
            const events = await getThemeEvents() 
        
            expect(_getThemesEvents).toHaveBeenCalledWith({}) 
            expect(events).toEqual([]) 
        }) 

        it('createThemeEvent posts the command body', async () => { 
            _postThemesEvents.mockResolvedValueOnce({ data: { id: 10 }, errors: null }) 
            
            await createThemeEvent(eventBody) 
            
            expect(_postThemesEvents).toHaveBeenCalledWith({ body: eventBody }) 
        }) 

        it('updateThemeEvent has all fields including id in the body', async () => {
            const updateRequestBody: Parameters<typeof updateThemeEvent>[0] = {
                id: 1,
                name: 'Viking Night', 
                themeId: 1, 
                goldMultiplier: 1.5, 
                expMultiplier: 2, 
                activeFromUtc: new Date('2026-06-16T18:00:00.000Z'), 
                activeUntilUtc: null, 
                requiredEquipmentSlotsMatchingTheme: ['Body', 'Weapon'],
                minimumThemedItemsEquipped: 2,
            }

            _putThemes.mockResolvedValueOnce({ data: updateRequestBody, errors: null }) 
            
            await updateThemeEvent(updateRequestBody) 
            
            expect(_putThemesEvents).toHaveBeenCalledWith({ body: updateRequestBody }) 
        }) 

        it('deleteThemeEvent deletes by id', async () => { 
            _deleteThemesEventsById.mockResolvedValueOnce({ data: undefined, errors: null }) 

            await deleteThemeEvent(9) 

            expect(_deleteThemesEventsById).toHaveBeenCalledWith({ path: { id: 9 } }) 
        }) 
    }) 
 }) 