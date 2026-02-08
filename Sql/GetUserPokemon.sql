-- 获取用户的Pokemon列表，支持分页和排序
SELECT 
    up.UserId,
    up.PokemonId,
    up.CaughtAt,
    up.Nickname,
    up.IsFavorite,
    up.Level,
    up.Experience,
    up.ExperienceToNextLevel,
    up.Health,
    up.MaxHealth,
    up.BattlesWon,
    up.BattlesLost,
    up.LastBattleAt,
    p.Name as PokemonName,
    p.Height,
    p.Weight,
    p.Order,
    p.Sprites
FROM UserPokemons up
INNER JOIN Pokemons p ON up.PokemonId = p.Id
WHERE up.UserId = @UserId
    AND (@FavoriteOnly = 0 OR up.IsFavorite = 1)
    AND (@MinLevel IS NULL OR up.Level >= @MinLevel)
    AND (@MaxLevel IS NULL OR up.Level <= @MaxLevel)
ORDER BY 
    CASE WHEN @SortBy = 'level' AND @SortOrder = 'asc' THEN up.Level END ASC,
    CASE WHEN @SortBy = 'level' AND @SortOrder = 'desc' THEN up.Level END DESC,
    CASE WHEN @SortBy = 'experience' AND @SortOrder = 'asc' THEN up.Experience END ASC,
    CASE WHEN @SortBy = 'experience' AND @SortOrder = 'desc' THEN up.Experience END DESC,
    CASE WHEN @SortBy = 'name' AND @SortOrder = 'asc' THEN p.Name END ASC,
    CASE WHEN @SortBy = 'name' AND @SortOrder = 'desc' THEN p.Name END DESC,
    CASE WHEN @SortBy = 'id' AND @SortOrder = 'asc' THEN up.PokemonId END ASC,
    CASE WHEN @SortBy = 'id' AND @SortOrder = 'desc' THEN up.PokemonId END DESC,
    CASE WHEN @SortBy = 'caughtAt' AND @SortOrder = 'asc' THEN up.CaughtAt END ASC,
    CASE WHEN @SortBy = 'caughtAt' AND @SortOrder = 'desc' THEN up.CaughtAt END DESC,
    up.CaughtAt DESC -- 默认排序
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;

-- 获取总数的查询
SELECT COUNT(*)
FROM UserPokemons up
WHERE up.UserId = @UserId
    AND (@FavoriteOnly = 0 OR up.IsFavorite = 1)
    AND (@MinLevel IS NULL OR up.Level >= @MinLevel)
    AND (@MaxLevel IS NULL OR up.Level <= @MaxLevel);