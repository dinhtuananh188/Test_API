CREATE OR REPLACE PROCEDURE insert_from_json(
    p_table_name VARCHAR,
    p_json_data JSON,
    INOUT p_row_count INTEGER
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_columns TEXT;
    v_select_columns TEXT;
BEGIN
    -- Lấy danh sách cột từ JSON (loại bỏ cột 'id' nếu có)
    SELECT string_agg(
        CASE
            WHEN json_key = 'id' THEN NULL
            ELSE '"' || replace(json_key, ' ', '_') || '"'
        END, ', '
    )
    INTO v_columns
    FROM json_object_keys(p_json_data->0) AS json_key
    WHERE json_key != 'id';

    -- Tạo danh sách cột cho SELECT từ JSON
    SELECT string_agg(
        CASE
            WHEN json_key = 'id' THEN NULL
            WHEN json_key ILIKE '%date%' OR json_key ILIKE '%study_from%' OR json_key ILIKE '%study_to%'
            THEN 'NULLIF(' || 'item->>' || quote_literal(json_key) || ', '''')::DATE'
            ELSE 'NULLIF(' || 'item->>' || quote_literal(json_key) || ', '''')'
        END, ', '
    )
    INTO v_select_columns
    FROM json_object_keys(p_json_data->0) AS json_key
    WHERE json_key != 'id';

    -- Thực hiện INSERT SELECT
    EXECUTE format(
        'INSERT INTO %I (%s) SELECT %s FROM json_array_elements(%L) AS item',
        p_table_name, v_columns, v_select_columns, p_json_data
    );

    -- Lấy số dòng được insert
    GET DIAGNOSTICS p_row_count = ROW_COUNT;
END;
$$;