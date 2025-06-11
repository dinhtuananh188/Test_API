DO $$
DECLARE
    row_count INTEGER;
BEGIN
    CALL insert_from_json(
        '123',
        '[{"STT":"1","student_code":"ABC123","full_name_vi":"Nguyen Van A","date_of_birth":"01/01/2000"}]',
        row_count
    );
    RAISE NOTICE 'Rows inserted: %', row_count;
END $$;